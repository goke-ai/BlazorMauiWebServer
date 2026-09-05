using CommunityToolkit.Mvvm.Input;
using GokeApp.Models;

namespace GokeApp.PageModels;

public interface IProjectTaskPageModel
{
	IAsyncRelayCommand<ProjectTask> NavigateToTaskCommand { get; }
	bool IsBusy { get; }
}