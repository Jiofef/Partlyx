using CommunityToolkit.Mvvm.Input;
using Partlyx.Core.Contracts;
using Partlyx.Infrastructure.Data.Implementations;
using Partlyx.Infrastructure.Events;
using Partlyx.ViewModels.UIServices.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.ViewModels.UIServices.Implementations
{
    public partial class MainWindowNameController : IDisposable
    {
        private readonly IMainWindowController _parent;
        private readonly ILocalizationService _loc;

        private readonly List<IDisposable> _subscriptions = new();

        public readonly string DefaultName = "Partlyx";

        private string? _cachedFileName;
        private string? _prefix;

        private string? _postfix;

        public MainWindowNameController(IMainWindowController parentController, ILocalizationService localizationService, IEventBus bus)
        {
            _parent = parentController;
            _loc = localizationService;

            var fileChangesSavedUpdatedSubscription = bus.Subscribe<FileChangesSavedUpdatedEvent>(ev => UpdatePrefix(ev));
            _subscriptions.Add(fileChangesSavedUpdatedSubscription);
            var fileNameChangedSubscription = bus.Subscribe<CurrentFileNameChangedEvent>(ev => UpdatePrefix(ev));
            _subscriptions.Add(fileNameChangedSubscription);
        }



        private void UpdateName()
        {
            string name = "";

            if (_prefix != null)
            {
                name += _prefix + " - ";
            }

            name += DefaultName;

            if (_postfix != null)
            {
                name += " - " + _postfix;
            }

            _parent.WindowTitle = name;
        }

        [RelayCommand]
        public void SetAsPostfix(string? postfix)
        {
            _postfix = postfix;

            UpdateName();
        }

        [RelayCommand]
        public void ClearPostfix()
        {
            _postfix = null;

            UpdateName();
        }

        private void UpdatePrefix(object ev)
        {
            string? newPrefix = null;

            string? prefixStar = null;

            if (ev is CurrentFileNameChangedEvent fileNameChangedEv)
            {
                _cachedFileName = fileNameChangedEv.newName;
            }

            if (ev is FileChangesSavedUpdatedEvent fileChangesEv)
            {
                if (!fileChangesEv.IsChangesSaved)
                    prefixStar = "*";
            }

            newPrefix = (_cachedFileName == null && prefixStar == null) ? null : _cachedFileName + prefixStar;

            _prefix = newPrefix;

            UpdateName();
        }

        private readonly string[] _randomPostfixes = 
            [
            "postfix_funny_programm_for_interesting_things",
            "postfix_Send_my_regards_to_David___"
            ];

        [RelayCommand]
        public void RandomizePostfix()
        {
            Random random = new Random();
            int index = random.Next(_randomPostfixes.Length);

            string newPostfix;

            // Prevent choosing the same post fix twice
            newPostfix = _loc[_randomPostfixes[index]];

            SetAsPostfix(newPostfix);
        }

        public void Dispose()
        {
            foreach (var subscription in  _subscriptions) 
                subscription.Dispose();
        }
    }
}
