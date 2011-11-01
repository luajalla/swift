#r @".\bin\Debug\SwiftTypeProvider.dll"

open Swift
open Swift.OptionTypes

/// Search for the predifined formats
type MT191 = SwiftMessage<191>
let m191 = MT191()
m191.TransactionReferenceNumber <- "103020112348RN104"
m191.RelatedReference <- "102820111409RN101"
m191.OrderingInstitution <- OptionAD.A(OptionA("BICMMGMGXXX"))
m191.Field71B <- "\BROK\\"

let f32B = new MT191.Field32B()
f32B.Field32B0 <- "AUD"
f32B.Field32B1 <- 152710.50M
m191.Field32B <- f32B

m191.ToString() //some optional fields are blank

/// Get SWIFT spec from handbook http://www2.anasys.com/swifthandbook/
type MT202 = SwiftMessage<202>
let m202 = MT202()
m202.AccountWithInstitution <- OptionABD.A(OptionA("FRNYUS33"))
m202.BeneficiaryInstitution <- OptionAD.D(OptionD("20061020050500001M02606", "BANK OF KIRIBATI LIMITED"))
m202.Intermediary <- OptionAD.A(OptionA("FRNYUS33XXX"))
m202.OrderingInstitution <- OptionAD.D(OptionD("CHENNAISUN BANK 12 SALEM KAMIR ROAD"))
m202.Receiver'sCorrespondent <- OptionABD.B(OptionB("20061020050500001M02608", "United States"))
m202.RelatedReference <- "NONREF"
m202.Sender'sCorrespondent <- OptionABD.A(OptionA("ANBIAGAG"))
m202.SenderToReceiverInformation <- "\42"
m202.TransactionReferenceNumber <- "103020112310RN102"

let dca = new MT202.ValueDateCurrencyCodeAmount()
dca.Amount <- 50000M
dca.CurrencyCode <- "USD"
dca.ValueDate <- 120418
m202.ValueDateCurrencyCodeAmount <- dca

let ti = new MT202.TimeIndicator()
ti.TimeIndicator0 <- "CLSTIME"
ti.TimeIndicator1 <- 0915
ti.TimeIndicator2 <- "+"
ti.TimeIndicator3 <- 0100 
m202.TimeIndicator <- ti

m202.ToString()
