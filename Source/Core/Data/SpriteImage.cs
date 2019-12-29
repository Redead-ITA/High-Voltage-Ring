
#region ================== Copyright (c) 2007 Pascal vd Heiden

/*
 * Copyright (c) 2007 Pascal vd Heiden, www.codeimp.com
 * This program is released under GNU General Public License
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 */

#endregion

#region ================== Namespaces

using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using CodeImp.DoomBuilder.IO;
using CodeImp.DoomBuilder.Windows;

#endregion

namespace CodeImp.DoomBuilder.Data
{
	public interface ISpriteImage //mxd
	{
		int OffsetX { get; }
		int OffsetY { get; }
	}

	public sealed class SpriteImage : ImageData, ISpriteImage
	{
		#region ================== Variables

		private int offsetx;
		private int offsety;
		
		#endregion

		#region ================== Properties

		public int OffsetX { get { return offsetx; } }
		public int OffsetY { get { return offsety; } }
		
		#endregion
		
		#region ================== Constructor / Disposer

		// Constructor
		internal SpriteImage(string name)
		{
			// Initialize
			SetName(name);

            AllowUnload = false;

			// We have no destructor
			GC.SuppressFinalize(this);
		}

		#endregion

		#region ================== Methods

		//mxd
		override public void LoadImage(bool notify)
		{
			// Do the loading
			base.LoadImage(false);

			// Notify the main thread about the change to redraw display
			if (notify) General.MainWindow.SpriteDataLoaded(this.Name);
		}

		// This loads the image
		protected override LocalLoadResult LocalLoadImage()
		{
            // Get the lump data stream
            Bitmap bitmap = null;
            string error = null;
			string spritelocation = string.Empty; //mxd
			Stream lumpdata = General.Map.Data.GetSpriteData(Name, ref spritelocation);
			if(lumpdata != null)
			{
				// Get a reader for the data
				IImageReader reader = ImageDataFormat.GetImageReader(lumpdata, ImageDataFormat.DOOMPICTURE, General.Map.Data.Palette);
				if(reader is UnknownImageReader)
				{
					// Data is in an unknown format!
					error = "Sprite lump \"" + Path.Combine(spritelocation, Name) + "\" data format could not be read. Does this lump contain valid picture data at all?";
					bitmap = null;
				}
				else
				{
                    // Read data as bitmap
                    lumpdata.Seek(0, SeekOrigin.Begin);
					if(bitmap != null) bitmap.Dispose();
					bitmap = reader.ReadAsBitmap(lumpdata, out offsetx, out offsety);
				}
					
				// Done
                lumpdata.Close();
			}
			else
			{
				// Missing a patch lump!
				error = "Missing sprite lump \"" + Name + "\". Forgot to include required resources?";
			}

            return new LocalLoadResult(bitmap, error, () =>
            {
                scale.x = 1.0f;
                scale.y = 1.0f;

                // Make offset corrections if the offset was not given
                if ((offsetx == int.MinValue) || (offsety == int.MinValue))
                {
                    offsetx = (int)((width * scale.x) * 0.5f);
                    offsety = (int)(height * scale.y);
                }
            });
        }

        #endregion
    }
}
