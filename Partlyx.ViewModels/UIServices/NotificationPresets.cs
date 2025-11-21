namespace Partlyx.ViewModels.UIServices
{
    public static class NotificationPresets
    {
        // Errors
        public static readonly NotificationErrorOptions InvalidFileErrorPreset = new("Invalid_file_error", "An_error_occured_during_opening_a_file_File_wasnt_opened");
        public static readonly NotificationErrorOptions SavingFileErrorPreset = new("Saving_error", "An_error_occured_during_saving_a_file_File_wasnt_saved");
        public static readonly NotificationErrorOptions InvalidImageLoadingErrorPreset = new("Invalid_image_error", "An_error_occured_during_loading_an_image_Image_wasnt_loaded");

        // Info

        // Confirm
        public static readonly NotificationConfirmOptions ExitingFileSaveConfirm = new("The_file_is_not_saved", "Do_you_want_to_save_your_file_before_leaving_");
        public static readonly NotificationConfirmOptions NewFileCreationFileSaveConfirm = new("The_file_is_not_saved", "Do_you_want_to_save_your_file_before_creating_a_new_one_");
        public static readonly NotificationConfirmOptions OtherFileOpenFileSaveConfirm = new("The_file_is_not_saved", "Do_you_want_to_save_your_file_before_opening_another_one");
    }
}
