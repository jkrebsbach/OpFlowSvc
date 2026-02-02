using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpFlow.Service.Test
{
	public class Sandbox
	{
		[Fact]
		public async Task TestImportFile()
		{
			try
			{
				var rootFolder = @"D:\ColdStorage\Documents\OpFlow\Documents\Count Sheets";

				string output = "tray_id, tray_name, instrument_name, manufacturer, instrument_type, quantity, category, vendor_tray\r\n";


				foreach (var file in Directory.GetFiles(rootFolder))
				{
					var dsExcel = ReadExcel(file);

					// Target columns
					//Cr	Vendor Name			Instrument Name				Reqd	Act1AddI		Miss

					var data = false;

					var idRow = dsExcel.Tables[0].Rows[2];

					string trayId = dsExcel.Tables[0].Rows[2][3]?.ToString();
					string trayName = Path.GetFileNameWithoutExtension(file);
					string instrumentType = "";
					string category = "";
					string vendorTray = "N";

					var trayInstruments = new List<TrayInstrumentXls>();

					for (var rowIndex = 1; rowIndex < dsExcel.Tables[0].Rows.Count; rowIndex++)
					{
						var sheetRow = dsExcel.Tables[0].Rows[rowIndex];



						if (sheetRow[0]?.ToString() == "Cr" && sheetRow[1]?.ToString() == "Vendor Name")
						{
							data = true;
							continue;
						}

						if (data)
						{
							var columnA = sheetRow[0]?.ToString();
							var manufacturer = sheetRow[1]?.ToString();
							var instrumentName = sheetRow[4]?.ToString();
							var quantityStr = sheetRow[8]?.ToString();

							if (columnA == "" && instrumentName != "" && quantityStr != "")
							{
								int quantity = int.Parse(quantityStr);

								instrumentName = instrumentName.Trim().Replace("\n", "-");

								var existing = trayInstruments.FirstOrDefault(x => x.instrumentName == instrumentName);
								if (existing != null)
								{
									existing.quantity += quantity;
									continue;
								}
								else
								{
									var ti = new TrayInstrumentXls()
									{
										instrumentName = instrumentName,
										quantity = quantity,
										instrumentType = instrumentType,
										manufacturer = manufacturer
									};
									trayInstruments.Add(ti);
								}
							}

							// we've reached the end?
							if (columnA.Contains("_________________") && manufacturer == "" && instrumentName == "" && quantityStr == "")
								data = false;
						}
					}


					foreach (var ti in trayInstruments)
					{
						output += $"{trayId}, {trayName}, {ti.instrumentName}, {ti.manufacturer}, {ti.instrumentType}, {ti.quantity}, {category}, {vendorTray}\r\n";
					}
				}

				File.WriteAllText(@"D:\temp\test.csv", output);
			}
			catch(Exception ex)
			{
				throw;
			}		
		}

		public class TrayInstrumentXls
		{
			public string instrumentName;
			public string manufacturer;
			public string instrumentType;
			public int quantity;
		}

		private static DataSet ReadExcel(string fileName)
		{
			//string CONNECTION_STRING = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=<FILENAME>;Extended Properties=\"Excel 8.0;HDR=Yes;\";";
			string CONNECTION_STRING = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=<FILENAME>;Extended Properties=\"Excel 8.0;HDR=Yes;\";";

			//OleDbConnection objConnection = new OleDbConnection();
			//objConnection = new OleDbConnection(CONNECTION_STRING.Replace("<FILENAME>", fileName));
			DataSet dsImport = new DataSet();

			string sheetName = null;

			try
			{
				//objConnection.Open();

				//DataTable dtSchema = objConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

				//if ((null == dtSchema) || (dtSchema.Rows.Count <= 0))
				//{
				//	//raise exception if needed
				//}

				//if ((null != sheetName) && (0 != sheetName.Length))
				//{
				//	//if (!CheckIfSheetNameExists(sheetName, dtSchema))
				//	//{
				//	//	//raise exception if needed
				//	//}
				//}
				//else
				//{
				//	//Reading the first sheet name from the Excel file.
				//	sheetName = dtSchema.Rows[0]["TABLE_NAME"].ToString();
				//}

				//new OleDbDataAdapter("SELECT * FROM [" + sheetName + "]", objConnection).Fill(dsImport);
			}
			catch (Exception)
			{
				//raise exception if needed
			}
			finally
			{
				//// Clean up.
				//if (objConnection != null)
				//{
				//	objConnection.Close();
				//	objConnection.Dispose();
				//}
			}


			return dsImport;
		}
	}
}
