# D2P Modelling Guideline

## (1) Basic Concepts

### Parametric models from design to production

- Rhino & Grasshopper are a perfect team to efficiently create parametric 3D-models.
- But: they can only deal with simple, geometric objects, and it is difficult to store and retreive intermediate results.
- For a multi-stage process from early conceptual design until production and assembly of hundreds of complex parts, this is often not sufficient.
- This is why [Design-to-Production](https://www.designtoproduction.com) has developed a modelling strategy that allows the *iterative creation* of parametric models with *complex components*.

### The core of parametric modelling

- Parametric modelling means to generate complex, detailed output from simple, abstract (geometrical and non-geometrical) input, based on defined rules.
- Intricate parametric models are created step-by-step: the output of one step serves as input for the next step. 
- Changes are easy, because every step can be repeated with different inputs.
- Scaling is easy, because the same rule can be applied to a multitude of inputs in parallel, creating a multitude of outputs.

### Encapsulation, types and instances: object-oriented parametrics

- To model complex building components, multiple geometric objects are ```encapsulated```, so that they can be identified and handled as an entity.
- To apply the same parametric rule to different building components, they all need to have the same, identifiable structure, defined by their ```component-type```.
- Every component in the model is an ```instance``` of a specific type and has a unique instance-name to be identified (and to create relations between instances).

### Parents, children and joints: structured models

- To create structured models with clear dependencies between components, a ```parent-instance``` can have multiple ```child-instances```. 
- This defines a parametric hierarchy: If the parent-instance is changed, all its children typically need to be updated.
- Joints between building parts are modeled as separate components: a ```joint-component``` is a child-instance with multiple parents.

## (2) D2P Components in Rhino

### A D2P-component is stored as a group of geometric objects with a uniqe name and a defined type

- All *Rhino objects* belonging to a component instance (short: a component) are members of a _Rhino group_. The group is not nested, its *group-name* does not matter.
- Every component has a unique name. This ```component-name``` is stored in the _object-name_ of all _Rhino objects_ belonging to the component. (If the grouping breaks, they can be found and re-grouped).
- The component-name consists of a ```type-id``` and an ```instance-name```. The instance name is unique for all instances of the same type. (It should be human-readable, following some logic of the model. It can be used to further structure the model)
- To identify specific ```geometric properties``` within the component (and make them available as input for parametric rules), the grouped _Rhino objects_ are stored in different _Rhino layers_. The layer structure is type-specific and all components of the same type use the same _Rhino-layers_.
- Every component contains one ```component-label```, a _Rhino text-object_ that shows the instance-name and defines a component-plane (a local coordinate system). 
- ```Non-geometric properties``` of the component can be stored as _user-text-attributes_ in the component-label
- Additional _Rhino objects_ can be added to the component, following the definitions of the component type.

### A component-type defines the layer structure of its component-instances

- Each type has a unique ```type-id``` , typically 4 uppercase letters that abbreviate the type-name and are easy to memorize.
- Each type has a ```type-name``` , a short human readable description of the type.
- Each type has a ```root-layer``` in the Rhino-model, named after type-id and type-name. The root-layer of a type contains the component-labels of all instances of that type.
- Each type can have a hierarchical ```layer-tree``` underneath its root-layer. All layer-names in the layer-tree start with the type-id of the component-type (the same as the root-layer). 
- Those layers contain **all** the geometric properties of **all** the type’s instances.
- “Empty” layers in the hierarchy are allowed, i.e. to group certain layers and organise the layer structure.
- All other layer properties (layer-color, print-color, etc.) are not relevant for the component-logic and can be used to organise the model and make it beautiful.
- All instances of a type store their instances in the same layer-tree, so they can be easily hidden, shown, locked, unlocked at once, completely or partially by manipulating the respective layer-status.

### Component-names, parent and child-instances

- The component-name consists of type-id and instance-name: [TYPE]:[name]
- Instance-names can be defined freely. They can be used to create a hierarchical parent-child-relationship between component instances, using a ```name-delimiter``` (typically a dot “.”):
  - The ```parent-instance``` is named “aa”
  - All ```child-instances``` are named “aa.01”, “aa.02”, “aa.03”, etc.
  - This can be used recursively “aa.01.x”, “aa.01.y”, etc.
- By searching for names, an instance can find its parent or all its children in the model.
- Note that, by definition, only the combination of type-id and instance-name need to be unique in the model. Thus, components of different types but with identical instance-names are allowed and can lead to mismatches when searching for parent- and child-instances.

### Joints between Components

- Joints between building-parts are modeled as separate components. This allows to work with abstract, undetailed components and joint-placeholders at the start of the project and then gradually refine the model until all joints are defined and detailed.
- ```joint-components``` are just components. But contrary to the above definition, their instances can have *multiple* parents: 
  - A joint component “connects” multiple part components - they can be found as its parents
  - Reversely, a part component "is connected" by multiple joints - they can be found as its children
- This is achieved by extending the naming conventions with a ```joint-delimiter```
  - This can be any character that **does not appear** in the component names or the regular ```name-delimiter```
  - A ```+``` sign is usually a safe choice, leading to names such as ```aa.01+bb.02``` for a joint instance connecting component instances ```aa.01``` and ```bb.02```
  - Specifically for cross joints in grid structures, ```x``` is often used as a more visual representation of two components crossing, e.g. ```01x02```
- If the same ```component-instances``` are connected by multiple joints, the joint names are distinguished by a ```joint-count```
  - Multiple joints for the above parts would be "aa.01+bb.02#1", "aa.01+bb02#2", "aa.01+bb02#3" etc.


