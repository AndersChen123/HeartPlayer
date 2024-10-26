using System.Globalization;
using HeartPlayer.ViewModels;

namespace HeartPlayer.Converters
{
    public class EnumToCollectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Enum.GetValues(value as Type ?? typeof(MainViewModel.SortOption))
                .Cast<Enum>()
                .Select(e => new KeyValuePair<string, Enum>(GetEnumDisplayName(e), e))
                .ToList();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private string GetEnumDisplayName(Enum enumValue)
        {
            return enumValue.ToString() switch
            {
                "NameAscending" => "Name (A-Z)",
                "NameDescending" => "Name (Z-A)",
                "CreationTimeAscending" => "Oldest First",
                "CreationTimeDescending" => "Newest First",
                _ => enumValue.ToString(),
            };
        }
    }
}
