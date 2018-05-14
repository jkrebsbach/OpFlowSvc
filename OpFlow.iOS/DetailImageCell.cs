using Foundation;
using OpFlow.iOS.Delegates;
using OpFlow.Mobile;
using System;
using UIKit;

namespace OpFlow.iOS
{
	public partial class DetailImageCell : UITableViewCell
    {
		private UIImage _image;
		private NSUrl _url;

        public DetailImageCell (IntPtr handle) : base (handle)
        {
		}

        public void UpdateCell(CaseImageToken token)
        {
			if (_image != null)
				return;

			try
			{
				var url = string.Empty;

                if (token.ImageType == CaseImageToken.ImageTypeEnum.FlowImage)
				{
					url = FlowUtil.FlowImageUrl(token.FolderID, token.DetailID);
				}
				else
				{
					url = SurgeryUtil.SurgeryImageUrl(token.FolderID, token.DetailID);
				}

				if (url == string.Empty)
					return;

				var nsUrl = new NSUrl(url);
                using (var data = NSData.FromUrl(nsUrl))
				{
					if (data != null)
					{
						_image = UIImage.LoadFromData(data);
					}

					ivDetail.Image = _image;
				}
				
			}
            catch(Exception ex)
			{
				var tmpInt = 0;
			}
        }
    }
}