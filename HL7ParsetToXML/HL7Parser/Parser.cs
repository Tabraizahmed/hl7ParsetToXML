using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HL7ParserModel;
using NextLevelSeven.Core;
using NextLevelSeven.Parsing;

namespace HL7Parser
{
    public class Parser
    {
        private delegate PatientDemoCondensed ParseFileContentToHl7(string fileContent);

        private delegate ChargesDemoCondensed ParseChargesFileContentToHl7(string fileContent);

        private ISegmentParser _pV1, _pId, _oBr, _iN1, _fT1;

        public async Task<List<SetOfPatientDemoAndCharges>> Parse(string folderPath)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);

            FileInfo[] files = directoryInfo.GetFiles("*.hl7");

            var filesContent = new List<SetOfPatientDemoAndCharges>();

            var objectToFile=new SetOfPatientDemoAndCharges();

            ParseFileContentToHl7 handler = ConvertFileStringToPatientInfoSegments;

            ParseChargesFileContentToHl7 chargesFileHandler = ConvertFileStringToChargesSegments;

            foreach (var file in files)
            {
                using (StreamReader streamReader = new StreamReader(file.FullName))
                {
                    // Need to log file name , which is going to process

                    var fileContent = await streamReader.ReadToEndAsync();

                    objectToFile.PatientDemoCondensed = handler(fileContent);

                    objectToFile.ChargesDemoCondensed = chargesFileHandler(fileContent);

                    filesContent.Add(objectToFile);

                    // Log file is being process successfully. 
                }
            }

            
            return filesContent;
        }

        private PatientDemoCondensed ConvertFileStringToPatientInfoSegments(string fileContent)
        {
            var message = Message.Parse(fileContent);
            try
            {

                _pV1 = message.Segments.FirstOrDefault(x => x.Type == "PV1");
                _pId = message.Segments.FirstOrDefault(x => x.Type == "PID");
                _oBr = message.Segments.FirstOrDefault(x => x.Type == "OBR");
                _iN1 = message.Segments.FirstOrDefault(x => x.Type == "IN1");


                var patientInfo = new PatientDemoCondensed
                {
                    AccessionId = _pV1.Field(19)?.Value,
                    FullName = _pId.Field(5).Value.Replace('^', ' '),
                    Address1 = _pId.Field(11).Value?.Split('^')[0],
                    Address2 = _pId.Field(11).Value?.Split('^')[1],
                    City = _pId.Field(11).Value?.Split('^')[2],
                    WorkZip = _pId.Field(11).Value?.Split('^').Last(),
                    BirthDate = _pId.Field(7)?.Value,
                    Gender = _pId.Field(8)?.Value[0],
                    PhoneHome = _pId.Field(13)?.Value?.Split('^')[0],
                    Ssn = _pId.Field(19)?.Value,
                    ZipCode = _pId.Field(11).Value?.Split('^').Last(),
                    LocationCode = ReArrangeLocationCode(_pV1.Field(3)?.Value),
                    MedicalRecord = _pId.Field(2)?.Value,
                    ProviderCode = _pV1.Field(7)?.Value.Split('^')[0],

                    ReferringPhys = _oBr?.Field(16).Value
                    .Split('^').LastOrDefault(),


                    InsuranceCode = _iN1.Field(3)?.Value,
                    InsPolicy = _iN1.Field(36)?.Value,
                    InsPolicyStatus = "P",
                    InsRelationShip = _iN1.Field(17)?.Value
                };

                return patientInfo;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            
        }

        private ChargesDemoCondensed ConvertFileStringToChargesSegments(string fileContent)
        {
            var message = Message.Parse(fileContent);

            _fT1 = message.Segments.FirstOrDefault(x => x.Type == "FT1");

            var chargesInfo = new ChargesDemoCondensed
            {
                DateFrom = _fT1.Field(4)?.Value,
                DateThru = _fT1.Field(4)?.Value,
                CptCode = _fT1.Field(25)?.Value?.Split('^')[0],
                UnitsForBilling = _fT1.Field(10)?.Value,
                RefLab = "UDLLAB",
            };
            return chargesInfo;
        }

        private string ReArrangeLocationCode(string rawLocationCode)
        {
            string locationCode = rawLocationCode.Replace('^', ' ').Trim();
            var indexOfAnd = locationCode.IndexOf('&')+1;
            return  locationCode.Substring(indexOfAnd,locationCode.Length-indexOfAnd).Insert(0,"ACC ");
        }
    }
}
