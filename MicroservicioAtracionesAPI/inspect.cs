using System;
using System.Reflection;
class Program {
    static void Main() {
        var asm = Assembly.LoadFrom(@"C:\Users\domea\.nuget\packages\microsoft.openapi\2.4.1\lib\net9.0\Microsoft.OpenApi.dll");
        foreach(var type in asm.GetTypes()) {
            if(type.Name.Contains("OpenApiInfo") || type.Name.Contains("OpenApiSecurity"))
                Console.WriteLine(type.FullName);
        }
    }
}
