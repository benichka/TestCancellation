using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TestCancellation.Commands;

namespace TestCancellation.ViewModel
{
    /// <summary>
    /// Main page ViewModel
    /// </summary>
    public class MainPageViewModel : INotifyPropertyChanged
    {
        private string _InfoMessage = string.Empty;
        /// <summary>Information to display to the user</summary>
        public string InfoMessage
        {
            get { return this._InfoMessage; }
            set
            {
                this.SetProperty(ref this._InfoMessage, value);
            }
        }

        /// <summary>Boolean indicating whether the main button has been pressed and the task is running</summary>
        private bool _IsProcessing = false;

        /// <summary>Cancellation token source</summary>
        private CancellationTokenSource _MainTaskTokenSource = null;

        /// <summary>Command associated with the main button</summary>
        public Command MainClickCommand { get; set; }

        /// <summary>Command associated with the cancel button</summary>
        public Command CancelCommand { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public MainPageViewModel()
        {
            this._MainTaskTokenSource = new CancellationTokenSource();

            this.MainClickCommand = new Command(this.MainClick, this.CanMainClick);

            this.CancelCommand = new Command(this.CancelClick, this.CanCancelClick);

            this.InfoMessage = "initialised";

            this._IsProcessing = false;
        }

        #region actions
        /// <summary>
        /// Method executed when the user clicks on the main button
        /// </summary>
        private async void MainClick()
        {
            this.InfoMessage = "The main button was clicked";

            this._IsProcessing = true;

            for (int i = 0; i < 10; i++)
            {
                if (this._MainTaskTokenSource.Token.IsCancellationRequested)
                {
                    // If a cancellation has been requested, we inform the user
                    // and abort the process
                    this.InfoMessage = $"Cancelled before processing i: {i}";
                    this._IsProcessing = false;

                    // Just before aborting, the source is regenerated
                    this._MainTaskTokenSource.Dispose();
                    this._MainTaskTokenSource = new CancellationTokenSource();
                    return;
                }

                this.InfoMessage = $"i: {i}";
                await Task.Delay(1000);
            }

            this.InfoMessage = "Processing done";

            this._IsProcessing = false;

            this.CheckCommands();
        }

        /// <summary>
        /// Method executed when the user clicks on the cancel button
        /// </summary>
        private void CancelClick()
        {
            this._MainTaskTokenSource.Cancel();

            // The processing is stopped; this automatically disabled the cancel button,
            // so that the user can only click it once
            this._IsProcessing = false;

            this.InfoMessage = "A cancellation has been requested";
        }

        /// <summary>
        /// Check if every command are executable
        /// </summary>
        private void CheckCommands()
        {
            this.MainClickCommand.RaiseCanExecuteChanged();
            this.CancelCommand.RaiseCanExecuteChanged();
        }
        #endregion actions

        #region controls
        /// <summary>
        /// Method to determine if the user can click on the main button
        /// </summary>
        /// <returns>true if that's the case, false otherwise</returns>
        private bool CanMainClick()
        {
            return !this._IsProcessing;
        }

        /// <summary>
        /// Method to determine if the user can cancel the action
        /// </summary>
        /// <returns>true if that's the case, false otherwise</returns>
        private bool CanCancelClick()
        {
            return this._IsProcessing;
        }
        #endregion controls

        #region event handling
        /// <summary>Event handler</summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raise the changing property event
        /// </summary>
        /// <param name="propertyName">Changing property</param>
        protected void RaisedPropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Field change notification only if the field has really changed
        /// </summary>
        /// <typeparam name="T">Field type</typeparam>
        /// <param name="storage">Initial value</param>
        /// <param name="value">Updated value</param>
        /// <param name="propertyName">Property name</param>
        /// <returns>true if the field value changed, false otherwise</returns>
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName]string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return false;
            }
            else
            {
                storage = value;
                this.RaisedPropertyChanged(propertyName);
                this.CheckCommands();
                return true;
            }
        }
        #endregion event handling
    }
}
