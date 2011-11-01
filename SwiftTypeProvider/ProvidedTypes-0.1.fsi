﻿// Copyright (c) Microsoft Corporation 2005-2011.
// This sample code is provided "as is" without warranty of any kind. 
// We disclaim all warranties, either express or implied, including the 
// warranties of merchantability and fitness for a particular purpose. 

// This file contains a set of helper types and methods for providing types in an implementation 
// of ITypeProvider.
//
// This code is a sample for use in conjunction with the F# 3.0 Developer Preview release of September 2011.


namespace Samples.FSharpPreviewRelease2011.ProvidedTypes

open System
open System.Reflection
open System.Linq.Expressions
open Microsoft.FSharp.Core.CompilerServices

/// Represents a provided parameter.
type ProvidedParameter =
    inherit System.Reflection.ParameterInfo
    new : parameterName: string * parameterType: Type * ?isOut:bool * ?optionalValue:obj -> ProvidedParameter
    
/// Represents a provided constructor.
type ProvidedConstructor =    
    inherit System.Reflection.ConstructorInfo

    /// Create a new provided constructor. It is not initially associated with any specific provided type definition.
    new : parameters: ProvidedParameter list -> ProvidedConstructor
    
    /// Add XML documentation information to this provided constructor
    member AddXmlDoc          : xmlDoc: string -> unit   
    
    /// Add XML documentation information to this provided constructor, where the computation of the documentation is delayed until necessary
    member AddXmlDocDelayed   : xmlDocFunction: (unit -> string) -> unit   
    
    /// Add XML documentation information to this provided constructor, where the documentation is re-computed  every time it is required.
    member AddXmlDocComputed   : xmlDocFunction: (unit -> string) -> unit   
    
    /// Set the quotation used to compute the implementation of invocations of this constructor.
    member InvokeCode         : (Quotations.Expr list -> Quotations.Expr) with set

    /// An alternative to InvokeCode that uses LINQ expressions to describe the code that implements invocations of this constructor.
    member InvokeCodeFromLinqExpression  : (ParameterExpression[] -> Expression) with get,set

    /// Add definition location information to the provided constructor.
    member AddDefinitionLocation : line:int * column:int * filePath:string -> unit
    

/// Represents a provided method.
type ProvidedMethod = 
    inherit System.Reflection.MethodInfo

    /// Create a new provided method. It is not initially associated with any specific provided type definition.
    new : methodName:string * parameters: ProvidedParameter list * returnType: Type * ?methodAttributes:MethodAttributes -> ProvidedMethod 

    /// Add XML documentation information to this provided constructor
    member AddXmlDoc            : xmlDoc: string -> unit    

    /// Add XML documentation information to this provided constructor, where the computation of the documentation is delayed until necessary
    member AddXmlDocDelayed   : xmlDocFunction: (unit -> string) -> unit   
    
    /// Add XML documentation information to this provided constructor, where the computation of the documentation is delayed until necessary
    /// The documentation is re-computed  every time it is required.
    member AddXmlDocComputed   : xmlDocFunction: (unit -> string) -> unit   
    
    member AddMethodAttrs       : attributes:MethodAttributes -> unit

    /// Get or set a flag indicating if the property is static.
    member IsStaticMethod       : bool with get, set

    /// Set the quotation used to compute the implementation of invocations of this constructor.
    member InvokeCode         : (Quotations.Expr list -> Quotations.Expr) with set

    /// Set the function used to compute the implementation of invocations of this constructor.
    member InvokeCodeFromLinqExpression         : (ParameterExpression[] -> Expression) with get,set

    /// Add definition location information to the provided type definition.
    member AddDefinitionLocation : line:int * column:int * filePath:string -> unit


/// Represents a provided property.
type ProvidedProperty =
    inherit System.Reflection.PropertyInfo

    /// Create a new provided type. It is not initially associated with any specific provided type definition.
    new  : propertyName: string * propertyType: Type * ?parameters : ProvidedParameter list -> ProvidedProperty

    /// Add XML documentation information to this provided constructor
    member AddXmlDoc            : xmlDoc: string -> unit    

    /// Add XML documentation information to this provided constructor, where the computation of the documentation is delayed until necessary
    member AddXmlDocDelayed   : xmlDocFunction: (unit -> string) -> unit   
    
    /// Add XML documentation information to this provided constructor, where the computation of the documentation is delayed until necessary
    /// The documentation is re-computed  every time it is required.
    member AddXmlDocComputed   : xmlDocFunction: (unit -> string) -> unit   
    
    /// Get or set a flag indicating if the property is static.
    member IsStatic             : bool with set

    /// Set the function used to compute the implementation of invocations of the getter method of this property.
    member GetterCode           : (Quotations.Expr list -> Quotations.Expr) with set

    /// Set the function used to compute the implementation of invocations of the getter method of this property, using LINQ expressions.
    member GetterCodeFromLinqExpression         : (ParameterExpression[] -> Expression) with get,set

    /// Set the function used to compute the implementation of invocations of the setter method of this property.
    member SetterCode           : (Quotations.Expr list -> Quotations.Expr) with set

    /// Set the function used to compute the implementation of invocations of the setter method of this property, using LINQ expressions.
    member SetterCodeFromLinqExpression         : (ParameterExpression[] -> Expression) with get,set

    /// Add definition location information to the provided type definition.
    member AddDefinitionLocation : line:int * column:int * filePath:string -> unit


/// Represents a provided literal field.
type ProvidedLiteralField =
    inherit System.Reflection.FieldInfo

    /// Create a new provided type. It is not initially associated with any specific provided type definition.
    new  : fieldName: string * fieldType: Type * literalValue: obj -> ProvidedLiteralField

    /// Add XML documentation information to this provided constructor
    member AddXmlDoc            : xmlDoc: string -> unit    

    /// Add XML documentation information to this provided constructor, where the computation of the documentation is delayed until necessary
    member AddXmlDocDelayed   : xmlDocFunction: (unit -> string) -> unit   
    
    /// Add XML documentation information to this provided constructor, where the computation of the documentation is delayed until necessary
    /// The documentation is re-computed  every time it is required.
    member AddXmlDocComputed   : xmlDocFunction: (unit -> string) -> unit   

    /// Add definition location information to the provided type definition.
    member AddDefinitionLocation : line:int * column:int * filePath:string -> unit


/// Represents a provided unit-of-measure annotation.
[<Class>]
type ProvidedMeasureBuilder =
    
    /// The ProvidedMeasureBuilder for building measures.
    static member Default : ProvidedMeasureBuilder

    /// e.g. 1
    member One : System.Type
    /// e.g. m * kg
    member Product : measure1:System.Type * measure2:System.Type  -> System.Type
    /// e.g. 1 / kg
    member Inverse : denominator:System.Type -> System.Type

    /// e.g. kg / m
    member Ratio : numerator:System.Type * denominator:System.Type -> System.Type
    
    /// e.g. m * m 
    member Square : ``measure``:System.Type -> System.Type
    
    /// the SI unit from the F# core library, where the string is in capitals and US spelling, e.g. Meter
    member SI : string -> System.Type
    
    /// e.g. float<kg>, Vector<int, kg>
    member AnnotateType : basic: System.Type * argument: System.Type list -> System.Type


/// Represents a provided static parameter.
type internal ProvidedStaticParameter =
    inherit System.Reflection.ParameterInfo

    /// Create a provided static parameter.
    new : parameterName: string * parameterType:Type * ?parameterDefaultValue:obj -> ProvidedStaticParameter

    /// Add XML documentation information to this provided constructor
    member AddXmlDoc            : xmlDoc: string -> unit    

    /// Add XML documentation information to this provided constructor, where the computation of the documentation is delayed until necessary
    member AddXmlDocDelayed   : xmlDocFunction: (unit -> string) -> unit   

/// Represents a provided type definition.
type ProvidedTypeDefinition =
    inherit System.Type

    /// Create a new provided type definition in a namespace. 
    new : assembly:Assembly * namespaceName:string * typeName:string * baseType:Type option -> ProvidedTypeDefinition

    /// Create a new provided type definition, to be located as a nested type in some type definition.
    new : typeName:string * baseType:Type option -> ProvidedTypeDefinition

    /// Add the given type as an implemented or inherited interface.
    member AddInterfaceImplementation : interfaceType:Type -> unit    

    /// Specifies that the given method body implements the given method declaration.
    member DefineMethodOverride : methodInfoBody:ProvidedMethod * methodInfoDeclaration:MethodInfo -> unit

    /// Add XML documentation information to this provided constructor
    member AddXmlDoc             : xmlDoc:string -> unit    

    /// Set the base type
    member SetBaseType             : baseType:Type -> unit    

    /// Add XML documentation information to this provided constructor, where the computation of the documentation is delayed until necessary.
    /// The documentation is only computed once.
    member AddXmlDocDelayed   : xmlDocFunction: (unit -> string) -> unit   
    
    /// Add XML documentation information to this provided constructor, where the computation of the documentation is delayed until necessary
    /// The documentation is re-computed  every time it is required.
    member AddXmlDocComputed   : xmlDocFunction: (unit -> string) -> unit   
    
    /// Set the attributes on the provided type. This fully replaces the default TypeAttributes.
    member SetAttributes        : System.Reflection.TypeAttributes -> unit
    
    /// Add a method, property, nested type or other member to a ProvidedTypeDefinition
    member AddMember         : memberInfo:MemberInfo      -> unit  

    /// Add a set of members to a ProvidedTypeDefinition
    member AddMembers        : memberInfos:list<#MemberInfo> -> unit

    /// Add a member to a ProvidedTypeDefinition, delaying computation of the members until required by the compilation context.
    member AddMemberDelayed  : memberFunction:(unit -> #MemberInfo)      -> unit

    /// Add a set of members to a ProvidedTypeDefinition, delaying computation of the members until required by the compilation context.
    member AddMembersDelayed : memberFunciton:(unit -> list<#MemberInfo>) -> unit    
    
    /// Add the types of the generated assembly as generative types, where types in namespaces get hierarchically positioned as nested types.
    member AddAssemblyTypesAsNestedTypesDelayed : assemblyFunction:(unit -> System.Reflection.Assembly) -> unit

    // Types with static parameters.
    member DefineStaticParameters     : parameters: ProvidedStaticParameter list * instantiationFunction: (string -> obj[] -> ProvidedTypeDefinition) -> unit

    /// Add definition location information to the provided type definition.
    member AddDefinitionLocation : line:int * column:int * filePath:string -> unit

    /// Suppress System.Object entries in intellisense menus in instances of this provided type 
    member HideObjectMethods  : bool with set

    /// Emit the given provided type definition and its nested type definitions into an assembly 
    /// and adjust the 'Assembly' property of all provided type definitions to return that assembly.
    ///
    /// The assembly is only emitted when the Assembly property on the root type is accessed for the first time.
    /// The host F# compiler does this when processing a generative type declaration for the type.
    member ConvertToGenerated : assemblyFileName: string -> unit

    /// Get or set a flag indicating if the ProvidedTypeDefinition is erased
    member IsErased : bool  with get,set

    /// Get or set a flag indicating if the ProvidedTypeDefinition has type-relocation suppressed
    [<Experimental("SuppressRelocation is a workaround and likely to be removed")>]
    member SuppressRelocation : bool  with get,set

/// A base type providing default implementations of type provider functionality when all provided 
/// types are of type ProvidedTypeDefinition.
type TypeProviderForNamespaces =

    /// Initializes a type provider to provide the types in the given namespace.
    new : namespaceName:string * types: ProvidedTypeDefinition list -> TypeProviderForNamespaces

    /// Initializes a type provider 
    new : unit -> TypeProviderForNamespaces

    /// Add a namespace of provided types.
    member AddNamespace : namespaceName:string * types: ProvidedTypeDefinition list -> unit

    /// Invalidate the information provided by the provider
    member Invalidate : unit -> unit

    interface ITypeProvider
