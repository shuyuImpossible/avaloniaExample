using CommunityToolkit.Mvvm.ComponentModel;

namespace avaloniaExample.ViewModels.SplitViewPane;

public partial class ButtonPageViewModel : ViewModelBase
{
    [ObservableProperty]
    private bool _isButtonEnabled = true;
}
