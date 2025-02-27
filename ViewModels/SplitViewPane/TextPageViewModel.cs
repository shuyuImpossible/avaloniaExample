using CommunityToolkit.Mvvm.ComponentModel;

namespace avaloniaExample.ViewModels.SplitViewPane;

public partial class TextPageViewModel : ViewModelBase
{
    [ObservableProperty]
    private bool _isTextEnabled = true;
}
