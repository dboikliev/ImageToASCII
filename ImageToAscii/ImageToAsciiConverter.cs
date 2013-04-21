using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

class ImageToAsciiConverter
{
    static void Main(string[] args)
    {
        //25 pixels of width and 50 pixels of height give enough space for every character to fit the box
        using (Bitmap image = new Bitmap(25, 50))
        {
            using (Graphics g = Graphics.FromImage(image))
            {
                //32 is the code of the first and 126 is the code of the last printable character in the ASCII table
                for (int symbolCode = 32; symbolCode <= 126; symbolCode++)
                {
                    g.FillRectangle(Brushes.White, 0, 0, image.Width, image.Height);

                    g.DrawString(((char)symbolCode).ToString(), (new Font("Consolas", 20)), Brushes.Black, 0, 0, StringFormat.GenericDefault);

                    g.Save();

                    //I noticed that there is some distortion when I use jpeg format which doesn't occur with bmp
                    image.Save("char" + symbolCode + ".bmp", ImageFormat.Bmp);
                }
            }
        }

        Dictionary<char, double> symbolsDarkness = new Dictionary<char, double>();
        for (int symbolCode = 32; symbolCode <= 126; symbolCode++)
        {
            char symbol = (char)symbolCode;
            using (Bitmap image = (Bitmap)Image.FromFile("char" + symbolCode + ".bmp"))
            {
                int nonWhitePixelsCount = 0;
                for (int y = 0; y < image.Height; y++)
                {
                    for (int x = 0; x < image.Width; x++)
                    {
                        Color pixel = image.GetPixel(x, y);

                        //A pixel is white only when all the channels have a value of 255
                        if (pixel.G != 255 || pixel.R != 255 || pixel.B != 255)
                        {
                            nonWhitePixelsCount++;
                        }
                    }
                }
                int pixelsInImageCount = image.Width * image.Height;
                double nonWhiteToWhiteRatio = (double)nonWhitePixelsCount / pixelsInImageCount;
                symbolsDarkness.Add(symbol, nonWhiteToWhiteRatio);
            }
        }

        var symbolsSortedByDarkness = from pair in symbolsDarkness
                                      orderby pair.Value ascending
                                      select pair;

        using (StreamWriter writer = File.CreateText("darkness.txt"))
        {
            foreach (KeyValuePair<char, double> pair in symbolsSortedByDarkness)
            {
                writer.WriteLine("{0,-5} {1,2} : {2:P}", (byte)pair.Key, pair.Key, pair.Value);
            }
        }

        using (Bitmap image = (Bitmap)Image.FromFile(@"C:\Users\Deyan\Desktop\203471_388914901176809_1896750110_q.jpg"))
        {
            using (StreamWriter writer = File.CreateText("ascii.txt"))
            {
                //The characters are sorted in descending order by their level of "darkness" (the element at position 0 is the darkest)
                char[] symbols = { '@', '%', '#', 'x', '+', '=', ':', '-', '.', ' ' };
                for (int y = 0; y < image.Height; y++)
                {
                    for (int x = 0; x < image.Width; x++)
                    {
                        Color pixel = image.GetPixel(x, y);
                        char symbol = '\0';

                        /*
                         * Some images may have some transparancy so I chose some symbol 
                         * that would be easily distinguished from the rest to represent transparency
                         */
                        if (pixel.A == 0)
                        {
                            symbol = '_';
                        }
                        else
                        {
                            /*
                             * The greyscale value is calculated according to
                             * the following formula
                             * GREY = 0.299 * RED + 0.587 * GREEN + 0.114 * BLUE
                             * For more information see http://en.wikipedia.org/wiki/Grayscale
                             */
                            double redPart = 0.299;
                            double greenPart = 0.587;
                            double bluePart = 0.114;
                            double greyShade = pixel.R * redPart + pixel.G * greenPart + pixel.B * bluePart;

                            /*
                             * greyShade / 255.0 gets the the amount of grey in the pixel
                             * 0 being black and 1 being white
                             * By multiplying this value times (symbols.Length - 1) I get
                             * the index of the symbol corresponding to the
                             * particular shade of grey
                             */
                            int saturationLevel = (int)((greyShade / 255.0) * (symbols.Length - 1));
                            symbol = symbols[saturationLevel];
                        }
                        writer.Write(symbol);
                    }
                    writer.WriteLine();
                }
            }
        }
    }
}