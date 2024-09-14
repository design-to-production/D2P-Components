using D2P_Core.Interfaces;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace D2P_GrasshopperTools.GH
{
    public abstract class GHVariableParameterComponent : GHComponentPreview, IGH_VariableParameterComponent
    {
        protected Dictionary<string, Type> _properties = new Dictionary<string, Type>();

        protected GHVariableParameterComponent(string name, string shortname, string description, string category, string subcategory)
        : base(name, shortname, description, category, subcategory)
        {

        }

        protected void SetDataTrees(IGH_DataAccess DA, GH_Structure<IGH_Goo> componentTree)
        {
            var outTrees = new List<GH_Structure<IGH_Goo>>();
            var typesDictionary = new Dictionary<string, int>();
            for (int i = 0; i < _properties.Count; i++)
            {
                typesDictionary.Add(_properties.Keys.ElementAt(i), i);
                outTrees.Add(new GH_Structure<IGH_Goo>());
            }

            for (int p = 0; p < componentTree.PathCount; p++)
            {
                var listData = componentTree.Branches[p];
                for (int i = 0; i < listData.Count; i++)
                {
                    var compWrapper = new GH_ObjectWrapper(listData[i]);
                    var component = compWrapper.Value as IComponent;
                    var typeIdx = typesDictionary[component.TypeID];
                    var path = new GH_Path(p);
                    outTrees[typeIdx].Append(compWrapper, path);
                }
            }

            for (int i = 0; i < outTrees.Count; i++)
            {
                DA.SetDataTree(i, outTrees[i]);
            }

        }

        protected bool OutputMismatch()
        {
            var countMatch = _properties.Count == Params.Output.Count;
            if (!countMatch) return true;

            foreach (var name in _properties)
            {
                if (!Params.Output.Select(p => p.NickName).Any(n => n == name.Key))
                {
                    return true;
                }
            }
            return false;
        }

        protected void CreateOutputParams(bool recompute)
        {
            var paramCount = _properties.Count;
            if (paramCount == 0) return;
            if (OutputMismatch())
            {
                if (Params.Output.Count < paramCount)
                {
                    while (Params.Output.Count < paramCount)
                    {
                        var new_param = CreateParameter(GH_ParameterSide.Output, Params.Output.Count);
                        Params.RegisterOutputParam(new_param);
                    }
                }
                else if (Params.Output.Count > paramCount)
                {
                    while (Params.Output.Count > paramCount)
                    {
                        Params.UnregisterOutputParameter(Params.Output[Params.Output.Count - 1]);
                    }
                }

                Params.OnParametersChanged();
                VariableParameterMaintenance();
                ExpireSolution(recompute);
            }
        }

        protected virtual bool OutputParamsAreValid()
        {
            return true;
        }

        public bool CanInsertParameter(GH_ParameterSide side, int index)
        {
            return false;
        }

        public bool CanRemoveParameter(GH_ParameterSide side, int index)
        {
            return false;
        }

        public IGH_Param CreateParameter(GH_ParameterSide side, int index)
        {
            return new Param_GenericObject();
        }

        public bool DestroyParameter(GH_ParameterSide side, int index)
        {
            return true;
        }

        public void VariableParameterMaintenance()
        {
            if (_properties == null) return;
            var names = _properties.Keys;
            for (var i = 0; i < Params.Output.Count; i++)
            {
                if (i > names.Count - 1) return;
                var name = names.ElementAt(i);
                var type = _properties[name];

                Params.Output[i].Name = $"{name}";
                Params.Output[i].NickName = $"{name}";
                Params.Output[i].Description = $"Component TypeID {name}";
                Params.Output[i].MutableNickName = false;
                if (type.IsAssignableFrom(typeof(IEnumerable)))
                {
                    Params.Output[i].Access = GH_ParamAccess.list;
                }
                else
                {
                    Params.Output[i].Access = GH_ParamAccess.item;

                }
            }
        }

        public override void ClearData()
        {
            base.ClearData();
            _properties?.Clear();
            if (Params == null || !Params.Any()) return;
            foreach (var ghParam in Params)
            {
                ghParam?.ClearData();
            }
        }
    }
}