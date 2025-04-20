using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;

namespace VTReplayConverter
{
    //Thank you Nebriv
    public class HeightMapGeneration
    {

        public Bitmap gameHeightmap;

        public static void ConvertHeightMap(string heightMapPath, string configPath)
        {
            Bitmap heightMap = FileDecoder.DecodeImage(heightMapPath);
            ConfigNode configFile = FileDecoder.LoadFromFile(configPath);
            int mapSize = configFile.GetValue<int>("mapSize");
            string scenarioName = configFile.GetValue<string>("scenarioName");
            scenarioName = scenarioName.Replace(' ', '_');
            scenarioName = scenarioName.Replace('/', '_');

            SaveHeightMap(heightMap, mapSize);
            GenerateMapXML(heightMap, mapSize, true);
        }

  
        private static void SaveHeightMap(Bitmap heightMap, int mapSize)
        {

            ConvertPngToRaw(heightMap);


            string outFile = Path.Combine(Program.TacviewTerrainPath, "VTOL_VR_CUSTOM_MAP.raw");

            Console.WriteLine("Height Map Info:");
            Console.WriteLine($"     Height: {heightMap.Height} Width: {heightMap.Width}");
            //Console.WriteLine($"     Latitude {0}, Longitude {0}");
            Console.WriteLine($"     Map Size {mapSize}");
            Console.WriteLine($"     Height Map Location: {outFile}");

            //File.WriteAllBytes(outFile, rawBytes);

        }

        static void ConvertPngToRaw(Bitmap bitmap)
        {
            string outFile = Path.Combine(Program.TacviewTerrainPath, "VTOL_VR_CUSTOM_MAP.raw");

            int width = bitmap.Width;
            int height = bitmap.Height;

            using (FileStream fs = new FileStream(outFile, FileMode.Create, FileAccess.Write))
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        System.Drawing.Color pixel = bitmap.GetPixel(x, y);

                        // Assume grayscale stored in R channel, scale to 16-bit
                        ushort value16 = (ushort)(pixel.R << 8 | pixel.R); // replicate 8-bit to 16-bit
                        // Write as big-endian
                        byte highByte = (byte)((value16 >> 8) & 0xFF);
                        byte lowByte = (byte)(value16 & 0xFF);
                        fs.WriteByte(highByte);
                        fs.WriteByte(lowByte);
                    }
                }
            }
            
        }

        private static void GenerateMapXML(Bitmap texture, int mapSize, bool customMap)
        {

            //Console.WriteLine("Generating custom XML");

            GeoLocation[] geoLocations = new GeoLocation[4];

            if (customMap)
            {
                geoLocations = GenerateCoords(mapSize);
            }
            else
            {
                GeoLocation bottomLeft = new GeoLocation
                {
                    Latitude = 53.94148616349887,
                    Longitude = -166.40063465537224
                };

                GeoLocation bottomRight = new GeoLocation
                {
                    Latitude = 53.94544,
                    Longitude = -165.426
                };

                GeoLocation topRight = new GeoLocation
                {
                    Latitude = 54.5124234999398,
                    Longitude = -165.41245674515972
                };

               GeoLocation topLeft = new GeoLocation
                {
                    Latitude = 54.51646078127723,
                    Longitude = -166.40063465537224
                };

                geoLocations[0] = bottomLeft;
                geoLocations[1] = bottomRight;
                geoLocations[2] = topRight;
                geoLocations[3] = topLeft;

            }

            int endian = 1;
            int width = texture.Width;
            int height = texture.Height;

            float altFactor = 0.0924f;
            float altOffset = -80f;

            string projection = "Quad";

            string xmlSavePath = Path.Combine(Program.TacviewTerrainPath, $"CustomHeightmapList.xml");
            XDocument doc = new XDocument(new XElement("Resources", new XElement("CustomHeightmapList", new XElement("CustomHeightmap",
                                                new XElement("File", $"VTOL_VR_CUSTOM_MAP.raw"),
                                                new XElement("BigEndian", endian.ToString()),
                                                new XElement("Width", width.ToString()),
                                                new XElement("Height", height.ToString()),
                                                new XElement("AltitudeFactor", altFactor.ToString()),
                                                new XElement("AltitudeOffset", altOffset.ToString()),
                                                new XElement("Projection", projection.ToString()),
                                                new XElement("BottomLeft",
                                                    new XElement("Longitude", geoLocations[0].Longitude),
                                                    new XElement("Latitude", geoLocations[0].Latitude)),
                                                new XElement("BottomRight",
                                                    new XElement("Longitude", geoLocations[1].Longitude),
                                                    new XElement("Latitude", geoLocations[1].Latitude)),
                                                new XElement("TopRight",
                                                    new XElement("Longitude", geoLocations[2].Longitude),
                                                    new XElement("Latitude", geoLocations[2].Latitude)),
                                                new XElement("TopLeft",
                                                    new XElement("Longitude", geoLocations[3].Longitude),
                                                    new XElement("Latitude", geoLocations[3].Latitude))
                                                ))));

            Console.WriteLine($"Saving custom tacview custom XML to {xmlSavePath}");
            doc.Save(xmlSavePath);

        }


        private static GeoLocation[] GenerateCoords(int mapSize)
        {
            GeoLocation[] geoLocations = new GeoLocation[4];


            GeoLocation bottomLeft = new GeoLocation();
            bottomLeft.Longitude = 0;
            bottomLeft.Latitude = 0;

            GeoLocation bottomRight = new GeoLocation();
            bottomRight.Longitude = ACMIUtils.WorldPositionToGPSCoords(new Vector3(0, 0, mapSize)).x;
            bottomRight.Latitude = ACMIUtils.WorldPositionToGPSCoords(new Vector3(0, 0, mapSize)).y;

            GeoLocation topLeft = new GeoLocation();
            topLeft.Longitude = ACMIUtils.WorldPositionToGPSCoords(new Vector3(mapSize, 0, 0)).x;
            topLeft.Latitude = ACMIUtils.WorldPositionToGPSCoords(new Vector3(mapSize, 0, 0)).y;

            GeoLocation topRight = new GeoLocation();
            topRight.Longitude = ACMIUtils.WorldPositionToGPSCoords(new Vector3(mapSize, 0, mapSize)).x;
            topRight.Latitude = ACMIUtils.WorldPositionToGPSCoords(new Vector3(mapSize, 0, mapSize)).y;

            geoLocations[0] = bottomLeft;
            geoLocations[1] = bottomRight;
            geoLocations[2] = topRight;
            geoLocations[3] = topLeft;




            return geoLocations;
        }

        public struct GeoLocation
        {
            public double Latitude { get; set; }
            public double Longitude { get; set; }

            public override string ToString()
            {
                return ($"{Latitude}, {Longitude}");
            }
        }

        public static GeoLocation FindPointAtDistanceFrom2(GeoLocation source, double bearing, double range)
        {
            const double EarthRadius = 6378137.0;
            const double DegreesToRadians = 0.0174532925;
            const double RadiansToDegrees = 57.2957795;

            double latA = source.Latitude * DegreesToRadians;
            double lonA = source.Longitude * DegreesToRadians;
            double angularDistance = range * 1000 / EarthRadius;
            double trueCourse = bearing * DegreesToRadians;

            double lat = Math.Asin(Math.Sin(latA) * Math.Cos(angularDistance) + Math.Cos(latA) * Math.Sin(angularDistance) * Math.Cos(trueCourse));

            double dlon = Math.Atan2(Math.Sin(trueCourse) * Math.Sin(angularDistance) * Math.Cos(latA), Math.Cos(angularDistance) - Math.Sin(latA) * Math.Sin(lat));
            double lon = ((lonA + dlon + Math.PI) % (Math.PI * 2)) - Math.PI;

            return new GeoLocation
            {
                Latitude = lat * RadiansToDegrees,
                Longitude = lon * RadiansToDegrees
            };

        }
    }
    
}
