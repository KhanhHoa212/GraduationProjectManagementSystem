using System;
using System.Reflection;
using System.Linq;

try {
    var assemblyPath = @"c:\Users\nguye\OneDrive\Documents\Spring26\PRN222\GraduationProjectManagementSystem\GPMS.Application\bin\Debug\net8.0\GPMS.Application.dll";
    var assembly = Assembly.LoadFrom(assemblyPath);
    var type = assembly.GetType("GPMS.Application.DTOs.UnscheduledGroupDto");
    if (type == null) {
        Console.WriteLine("Type not found.");
        return;
    }
    Console.WriteLine($"Type: {type.FullName}");
    var props = type.GetProperties().Select(p => p.Name).ToList();
    Console.WriteLine("Properties: " + string.Join(", ", props));
} catch (Exception ex) {
    Console.WriteLine("Error: " + ex.Message);
}
