namespace Partlyx.ViewModels.UIServices
{
    public static class NotificationPresets
    {
        // Errors
        public static readonly NotificationErrorOptions InvalidFileErrorPreset = new("Invalid file error", "An error occured during opening a file. File wasn't opened");
        public static readonly NotificationErrorOptions SavingFileErrorPreset = new("Saving error", "An error occured during saving a file. File wasn't saved");

        // Info

        // Confirm
        public static readonly NotificationConfirmOptions ExitingFileSaveConfirm = new("The file is not saved", "Do you want to save your file before leaving?");
        public static readonly NotificationConfirmOptions NewFileCreationFileSaveConfirm = new("The file is not saved", "Do you want to save your file before creating a new one?");
    }
}
