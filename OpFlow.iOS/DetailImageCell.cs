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
		
        public DetailImageCell (IntPtr handle) : base (handle)
        {
		}

        public async void UpdateCell(CaseImageToken token)
        {
			if (_image != null)
				return;

			try
			{
                byte[] imageBytes;

                if (token.ImageType == CaseImageToken.ImageTypeEnum.FlowImage)
				{
                    imageBytes = await FlowUtil.FlowImageBytes(token.FolderID, token.DetailID);
				}
				else
				{
                    imageBytes = await SurgeryUtil.SurgeryImageBytes(token.FolderID, token.DetailID);
				}

				if (imageBytes == null)
					return;

				using (var data = NSData.FromArray(imageBytes))
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