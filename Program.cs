using System;
using System.IO;
using System.Runtime.CompilerServices;

class Program
{
    static int GetNonNegativeInt(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string? raw = Console.ReadLine()?.Trim();

            if (int.TryParse(raw, out int value))
            {
                if (value <= 0)
                {
                    Console.WriteLine("Please enter a positive integer.");
                    continue;
                }
                return value;
            }
            Console.WriteLine("Please enter a whole number.");
        }
    }

    static string SafeName(string? name)
    {
        name = (name ?? "").Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.");

        return name;
    }

    static void Main()
    {
        try
        {
            Console.WriteLine("=== Folder Maker ===");

            Console.Write("Enter the base path where folders will be created (press Enter for current directory): ");
            string? baseDirInput = Console.ReadLine()?.Trim();

            string rootParent = string.IsNullOrWhiteSpace(baseDirInput) 
                ? Directory.GetCurrentDirectory() 
                : baseDirInput;

            string mainFolderName = SafeName(ReadPrompt("Enter the name of the main folder: "));
            string rootPath = Path.Combine(rootParent, mainFolderName);

            if (Directory.Exists(rootPath))
            {
                Console.WriteLine($"That folder already exists: {rootPath}");
                Console.Write("Continue and add subfolders inside it? (y/n): ");
                string? overwrite = Console.ReadLine()?.Trim().ToLowerInvariant();

                if (overwrite != "y")
                {
                    Console.WriteLine("Operation cancelled.");
                    return;
                }
            }
            else
            {
                Directory.CreateDirectory(rootPath);
                Console.WriteLine($"Created main folder: {rootPath}");
            }

            int count = GetNonNegativeInt("Enter the number of subfolders to create: ");

            for (int i = 0; i < count; i++)
            {
                string subName = SafeName(ReadPrompt($"Enter the name of subfolder #{i + 1}: "));
                string subPath = Path.Combine(rootPath, subName);

                Directory.CreateDirectory(subPath);
                Console.WriteLine($"Created subfolder: {subPath}");
            }

            Console.WriteLine("All folders created successfully.");
        }

        catch (ArgumentException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        catch (UnauthorizedAccessException)
        {
            Console.WriteLine("Error: You do not have permission to create folders in this location.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected Error: {ex.Message}");
        }
    }

    static string? ReadPrompt(string prompt)
    {
        Console.Write(prompt);
        return Console.ReadLine();
    }
}
