using Quickbird.Models;
using Quickbird.Util;
using DatabasePOCOs.User;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace Quickbird.ViewModels
{
    using Models;
    using Util;

    public class AddYieldViewModel : ViewModelBase
    {
        MainDbContext _db = new MainDbContext();
        private Guid _cropCycleID;
        private Windows.UI.Xaml.Visibility _errorVisibility = Windows.UI.Xaml.Visibility.Collapsed;
        private Windows.UI.Xaml.Media.SolidColorBrush _textBoxColour = new Windows.UI.Xaml.Media.SolidColorBrush
        {
            Color = Colors.LightCyan
        };
        private bool _validEntry = true;
        private double _userEnteredAmount;
        private string _buttonText = "Add Yield";
        private bool _closeCropRun = false;
        private Action<string> _updateAction;
        private CropCycle _cropCycle;
        private bool _isLoading = false; 
         

        public AddYieldViewModel(Guid CropCycleID)
        {
            _cropCycleID = CropCycleID;
            _updateAction = UpdateData; 
            Messenger.Instance.TablesChanged.Subscribe(_updateAction); 
            UpdateData(string.Empty); 
        }

        /// <summary>
        /// Attached to tables changed event
        /// </summary>
        /// <param name=""></param>
        private async void UpdateData(string input)
        {
            _cropCycle = await _db.CropCycles.FirstAsync(cc => cc.ID == _cropCycleID);
            if(_cropCycle.EndDate != null)
            {
                //TODO Close this frame bacuse the crop cycle is already closed! 
            }
        }

        /// <summary>
        /// Updated from UI throught Two-wauy binding, processes user input
        /// </summary>
        public string UserEnteredText
        {
            get
            {
                return string.Empty; 
            }
            set
            {
                bool success = Double.TryParse(value, out _userEnteredAmount);
                if(String.IsNullOrWhiteSpace(value))
                {
                    success = true;
                    _userEnteredAmount = 0; 
                }

                //Only take action if something has changed!                
                if (success != ValidEntry && success)
                {
                    ValidEntry = success;
                    ErrorVisibility = Windows.UI.Xaml.Visibility.Collapsed;
                    TextBoxColour = new Windows.UI.Xaml.Media.SolidColorBrush
                    {
                        Color = Colors.LightCyan
                    };
                }
                else if (success != ValidEntry && success == false)
                {
                    ValidEntry = success;
                    ErrorVisibility = Windows.UI.Xaml.Visibility.Visible;
                    TextBoxColour =  new Windows.UI.Xaml.Media.SolidColorBrush
                    {
                        Color = Colors.PaleVioletRed
                    }; 
                }
            }
        }

        /// <summary>
        /// Updated from UI through two-way binding, determines if the crop run will be closed 
        /// </summary>
        public bool CloseCropRun
        {
            get
            {
                return _closeCropRun;
            }
            set
            {
                _closeCropRun = value;
                ButtonText = value ? "End Crop" : "Add Yield";
            }
        }

        /// <summary>
        /// Runs when the user licks the button
        /// </summary>
        public async Task SaveCropRun()
        {
            ValidEntry = false;
            IsLoading = true; 
            _cropCycle.Yield += _userEnteredAmount;
            if (_closeCropRun)
                _cropCycle.EndDate = DateTimeOffset.Now;
            _cropCycle.UpdatedAt = DateTimeOffset.Now; 
            await _db.SaveChangesAsync();
            _updateAction = null; 
            await Messenger.Instance.TablesChanged.Invoke(string.Empty);
            _db.Dispose();
            IsLoading = false; 
        }

        public string ButtonText
        {
            get {
                return _buttonText; }
            set {
                _buttonText = value;
                OnPropertyChanged();
            }
        }

        public bool IsLoading
        {
            get
            {
                return _isLoading;
            }
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Goes true when user entered valid input, and lets hium proceed
        /// </summary>
        public bool ValidEntry
        {
            get {
                return _validEntry;
            }
            set {
                _validEntry = value;
                OnPropertyChanged(); 
            }
        }

        /// <summary>
        /// Goes red when user input is invalid 
        /// </summary>
        public Windows.UI.Xaml.Media.SolidColorBrush TextBoxColour {
            get {
                return _textBoxColour; 
            }
            set {
                _textBoxColour = value;
                OnPropertyChanged(); 
            }
        }

        public Windows.UI.Xaml.Visibility ErrorVisibility
        {
            get {
                return _errorVisibility;
            }
            set {
                _errorVisibility = value;
                OnPropertyChanged();
            }
        }
            
    }
}
