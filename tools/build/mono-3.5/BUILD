load('/tools/build/csharp', 'csharp_reference')

package(default_visibility = ['//visibility:public'])

csharp_reference(name = 'mscorlib', file = '//lib:ksp/KSP_Data/Managed/mscorlib.dll')
csharp_reference(name = 'System', file = '//lib:ksp/KSP_Data/Managed/System.dll')
csharp_reference(name = 'System.Core', file = '//lib:ksp/KSP_Data/Managed/System.Core.dll')
#csharp_reference(name = 'System.Runtime', file = '//lib:mono-4.5/Facades/System.Runtime.dll')
#csharp_reference(name = 'System.IO', file = '//lib:mono-4.5/Facades/System.IO.dll')
csharp_reference(name = 'System.Xml', file = '//lib:ksp/KSP_Data/Managed/System.Xml.dll')

csharp_reference(name = 'Google.Protobuf', file = '@csharp_protobuf_net35//:file/Google.Protobuf.dll')
csharp_reference(name = 'Newtonsoft.Json', file = '@csharp_json//:lib/net35/Newtonsoft.Json.dll')
csharp_reference(name = 'NDesk.Options', file = '@csharp_options//:lib/NDesk.Options.dll')
csharp_reference(name = 'Moq', file = '@csharp_moq//:lib/net35/Moq.dll')
