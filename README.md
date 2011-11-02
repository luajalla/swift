This is a **SWIFT Type Provider** for the simple messages.
It supports user defined schemes (offline) and online search for the specification.

#####Usage

The types are available with *SwiftTypeProvider.dll*.

For predefined formats SwiftMessage type with int parameter (the message number) is used:

    type MT191 = SwiftMessage<191>
    let m191 = MT191() //create message object
    m191.TransactionReferenceNumber <- "103020112348RN104"
    m191.RelatedReference <- "102820111409RN101"
    ...

If some requested message wasn't found the specification is downloaded from [SWIFT Handbook](http://www2.anasys.com/swifthandbook/).

    type MT202 = SwiftMessage<202>

Offline files can be used too. In this case you need to specify the path

    type MT202offline = SwiftMessage<202, "..\\offline\\fmt202.html">

A field consists of a sequence of components with a starting field tag and delimiters. There're two types of the fields: _Optional_ and _Mandatory_. When not specifying optional fields are omitted. `ToString()` call allows to see the message body:

    :20:103020112348RN10
    :21:102820111409RN10
    :32B:AUD152710.5
    :52A:BICMMGMGXXX
    :71B:\BROK\"


#####Implementation

The message type and subtypes are erased to `obj[]` to emulate mutable properties. Subtypes are created for complex fields (for example, 32A - Value Date, Currency Code, Amount)

    let dca = new MT202.ValueDateCurrencyCodeAmount()
    dca.Amount <- 50000M
    dca.CurrencyCode <- "USD"
    dca.ValueDate <- 120418
    m202.ValueDateCurrencyCodeAmount <- dca

Spec doesn't provide the names for all of the fields. In this case the subtype properties are indexed: 

    let ti = new MT202.TimeIndicator()
    ti.TimeIndicator0 <- "CLSTIME"
    ti.TimeIndicator1 <- 0915
    ti.TimeIndicator2 <- "+"
    ti.TimeIndicator3 <- 0100

For empty names the field name is in format _Field(tag)_, for example `Field32B`.

Union types are not supported yet so the types for some field options (A, B, D and their combinations) are there. 

The current implementation of type provider is intended for the simple messages, so it has several limitations: subsequences, optional field content (e.g. [8c]), qualifiers. As a workaround for the most of cases they can be expressed with a custom format.

Check _Script.fsx_ to test it.
