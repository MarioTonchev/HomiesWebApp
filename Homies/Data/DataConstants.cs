namespace Homies.Data
{
	public static class DataConstants
	{
		public const string RequiredErrorMessage = "The field {0} is required";
		public const string StringLengthErrorMessage = "The field {0} must be between {2} and {1} characters long";

		//Event
		public const int EventMaxName = 20;
		public const int EventMinName = 5;

		public const int EventMaxDescription = 150;
		public const int EventMinDescription = 15;

		public const string DateFormat = "yyyy-MM-dd H:mm";

		//Type
		public const int TypeMaxName = 15;
		public const int TypeMinName = 5;
	}
}
