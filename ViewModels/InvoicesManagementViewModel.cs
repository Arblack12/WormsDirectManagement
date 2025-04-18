using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using WormsDirectManagement.Helpers;
using WormsDirectManagement.Models;
using WormsDirectManagement.Services;

namespace WormsDirectManagement.ViewModels
{
    internal partial class InvoicesManagementViewModel : ObservableObject
    {
        private readonly string _configPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "invoices_config.json");

        [ObservableProperty] private string _status = "Idle";

        public ObservableCollection<SenderRule> Rules { get; } = new();

        public RelayCommand AddRuleCmd { get; }
        public RelayCommand EditRuleCmd { get; }
        public RelayCommand RemoveRuleCmd { get; }
        public RelayCommand SaveCmd { get; }
        public RelayCommand ProcessCmd { get; }

        public InvoicesManagementViewModel()
        {
            if (File.Exists(_configPath))
            {
                var list = JsonConvert.DeserializeObject<SenderRule[]>(File.ReadAllText(_configPath))
                           ?? Array.Empty<SenderRule>();
                foreach (var r in list) Rules.Add(r);
            }

            AddRuleCmd = new RelayCommand(Add);
            EditRuleCmd = new RelayCommand(Edit, () => Rules.Count > 0);
            RemoveRuleCmd = new RelayCommand(Remove, () => Rules.Count > 0);
            SaveCmd = new RelayCommand(Save);
            ProcessCmd = new RelayCommand(ProcessInvoices);
        }

        private void Add()
        {
            Rules.Add(new SenderRule());
        }

        private void Edit()
        {
            // keep simple: editing happens inline in the DataGrid.
        }

        private void Remove()
        {
            if (Rules.Count == 0) return;
            Rules.RemoveAt(Rules.Count - 1);
        }

        private void Save()
        {
            File.WriteAllText(_configPath, JsonConvert.SerializeObject(Rules, Formatting.Indented));
            Status = "Saved " + DateTime.Now;
        }

        private void ProcessInvoices()
        {
            Status = "Processing…";
            Save();                     // ensure current edits are stored
            var report = InvoiceProcessor.Process();
            MessageBox.Show(string.IsNullOrWhiteSpace(report) ? "Done" : report,
                            "Process Invoices", MessageBoxButton.OK, MessageBoxImage.Information);
            Status = "Done";
        }
    }
}
