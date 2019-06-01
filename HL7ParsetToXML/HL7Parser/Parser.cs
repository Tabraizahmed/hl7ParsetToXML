using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HL7ParserModel;
using NextLevelSeven.Core;

namespace HL7Parser
{
    public class Parser
    {
        private delegate PatientDemoCondensed ParseFileContentToHl7(string fileContent);
        public async Task<List<PatientDemoCondensed>> GetXmlFileContentToWrite(string folderPath)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);
            FileInfo[] files = directoryInfo.GetFiles("*.hl7");
            var filesContent = new List<PatientDemoCondensed>();
            ParseFileContentToHl7 handler = CovertFileStringToHl7Segments;

            foreach (var file in files)
            {
                using (StreamReader streamReader = new StreamReader(file.FullName))
                {
                    // Need to log file name , which is going to process

                    var fileContent = await streamReader.ReadToEndAsync();

                    var hl7Segment = handler(fileContent);

                    filesContent.Add(hl7Segment);

                    // Log file is being process successfully. 
                }
            }

            return filesContent;
        }

        private PatientDemoCondensed CovertFileStringToHl7Segments(string fileContent)
        {
            var message = Message.Parse(fileContent);
            try
            {

            

                var patientInfo = new PatientDemoCondensed();

                patientInfo.AccessionId = message[4][19]?.Value;
                patientInfo.FullName = message.Segment(3).Field(5).Value.Replace('^', ' ');
                patientInfo.Address1 = message.Segment(3).Field(11).Value?.Split('^')[0];
                patientInfo.Address2 = message.Segment(3).Field(11).Value?.Split('^')[1];
                patientInfo.City = message.Segment(3).Field(11).Value?.Split('^')[2];
                patientInfo.WorkZip = message.Segment(3).Field(11).Value?.Split('^').Last();
                patientInfo.BirthDate = message.Segment(3).Field(7)?.Value;
                patientInfo.Gender = message.Segment(3).Field(8)?.Value[0];
                patientInfo.PhoneHome = message.Segment(3).Field(13)?.Value;
                patientInfo.Ssn = message.Segment(3).Field(19)?.Value;
                patientInfo.ZipCode = message.Segment(3).Field(11).Value?.Split('^').Last();
                patientInfo.LocationCode = message.Segment(4).Field(3)?.Value;
                patientInfo.MedicalRecord = message.Segment(3).Field(2)?.Value;
                patientInfo.ProviderCode = message.Segment(4).Field(7)?.Value.Split('^')[0];

                patientInfo.ReferringPhys = message.Segments.FirstOrDefault(x => x.Type == "OBR")?.Field(16).Value
                    .Split('^').LastOrDefault();
                  

                patientInfo.InsuranceCode = message.Segment(5).Field(3)?.Value;
                patientInfo.InsPolicy = message.Segment(5).Field(36)?.Value;
                patientInfo.InsPolicyStatus = "P";
                patientInfo.InsRelationShip = message.Segment(5).Field(17)?.Value;

                return patientInfo;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
