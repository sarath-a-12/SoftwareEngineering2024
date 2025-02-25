﻿/*************************************************************************************
* Filename    = ToolListViewModel.cs
*
* Author      = Garima Ranjan
*
* Product     = Updater
* 
* Project     = Lab Monitoring Software
*
* Description = View Model for displaying available analyzers information on the UI
**************************************************************************************/

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using Updater;

namespace ViewModel.UpdaterViewModel;

/// <summary>
/// Class to populate list of available analyzers for server-side operations
/// </summary>
public class ToolListViewModel : INotifyPropertyChanged
{
    public ObservableCollection<Tool>? AvailableToolsList { get; set; }

    /// <summary>
    /// Loads available analyzers from the specified folder using the DllLoader.
    /// Populates the AnalyzerInfo property with the retrieved data.
    /// </summary>
    public ToolListViewModel(string? folder = null)
    {
        if (folder == null)
        {
            folder = AppConstants.ToolsDirectory;
        }
        LoadAvailableTools(folder);
    }
    /// <summary>
    /// Loads tools from the specified folder, ensuring only the latest versions
    /// of tools are added to the list. Updates the UI via data binding.
    /// </summary>
    /// <param name="folder">Optional folder path to load tools from.</param>
    public void LoadAvailableTools(string? folder = null)
    {
        if (folder == null)
        {
            folder = AppConstants.ToolsDirectory;
        }
        var dllLoader = new ToolAssemblyLoader();
        Dictionary<string, List<string>> hashMap = dllLoader.LoadToolsFromFolder(folder);

        if (hashMap.Count > 0)
        {
            int rowCount = hashMap.Values.First().Count;
            AvailableToolsList = [];

            for (int i = 0; i < rowCount; i++)
            {
                var newTool = new Tool {
                    ID = hashMap["Id"][i],
                    Name = hashMap["Name"][i],
                    Version = hashMap["Version"][i],
                    Description = hashMap["Description"][i],
                    Deprecated = hashMap["IsDeprecated"][i],
                    CreatedBy = hashMap["CreatorName"][i],
                    CreatorEmail = hashMap["CreatorEmail"][i],
                    LastModified = hashMap["LastModified"][i],
                    LastUpdated = hashMap["LastUpdated"][i]
                };
                // Check if a tool with the same unique key exists
                Tool? existingTool = AvailableToolsList.FirstOrDefault(tool =>
                tool.Name == newTool.Name &&
                tool.CreatedBy == newTool.CreatedBy &&
                tool.CreatorEmail == newTool.CreatorEmail);

                if (existingTool != null)
                {
                    // Compare versions
                    if (Version.Parse(newTool.Version) > Version.Parse(existingTool.Version))
                    {
                        // Remove the older version from the list
                        AvailableToolsList.Remove(existingTool);
                        AvailableToolsList.Add(newTool);
                        Trace.WriteLine($"[Updater] Replaced older version with new version for tool: {newTool.Name}");
                    }
                    else
                    {
                        Trace.WriteLine($"[Updater] Skipped adding an older version of tool: {newTool.Name}");
                    }
                }
                else
                {
                    // No existing tool with the same unique key; add new tool
                    AvailableToolsList.Add(newTool);
                    Trace.WriteLine($"[Updater] Added new tool: {newTool.Name}");
                }
            }
            Trace.WriteLine("Available Tools information updated successfully");
        }
        else
        {
            Trace.WriteLine("No files found");
        }

        OnPropertyChanged(nameof(AvailableToolsList));
    }
    /// <summary>
    /// Event triggered when a property value changes to notify the UI.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string? propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
