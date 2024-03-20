namespace Bookify.Web.Core.ViewModels
{
	public class RentalCopyViewModel
	{
        public BookCopyViewModel? BookCopyViewModel { get; set; }
        public BookCopy? BookCopy { get; set; }
		public DateTime RentalDate { get; set; }
		public DateTime EndDate { get; set; } 
		public DateTime? ReturnDate { get; set; }
		public DateTime? ExtendedOn { get; set; }

        public int DelayInDays {
			get
			{
				var delay = 0;
				//if return date is more than end date
				if(ReturnDate.HasValue && ReturnDate.Value > EndDate)
					delay = (int)(ReturnDate.Value - EndDate).TotalDays;

				//if subscriber hasn't return book yet and passed return date
				else if (!ReturnDate.HasValue && DateTime.Today > EndDate)
					delay = (int)(DateTime.Today - EndDate).TotalDays;

				return delay;
			}
			
		 }
    }

}
