namespace Partlyx.ViewModels.ItemProperties
{
    public class SpinBoxItemPropertyViewModel : ItemPropertyViewModel
    {
        private decimal _value;
        public decimal Value { get => _value; 
            set
            {
                decimal correctedValue =
                _step != 0
                ? (value / _step) * _step
                : value;
                SetProperty(ref _value, correctedValue);
            } }

        private decimal _step;
        public decimal Step { get => _step; 
            set
            {
                SetProperty(ref _step, value);

                // Updating value
                Value = Value;
            } }
    }
}
