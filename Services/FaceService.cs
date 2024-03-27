using FaceApp.Data;
using FaceApp.Models;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FaceApp.Services
{
    public class FaceService : IDisposable
    {
        private readonly FaceAppDBContext _context;
        public FaceService(FaceAppDBContext context)
        {
            _context = context;
        }

        public void Dispose()
        {
            _context.Dispose();
        }
        public void addperson(Person[] persons,string personname)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    PersonDetails personDetails = new PersonDetails();
                    personDetails.PersonId = persons[0].PersonId;
                    personDetails.PersonName = personname;
                    _context.Add(personDetails);
                    _context.PersonDetails.Add(personDetails);
                    _context.SaveChanges();

                }
                catch (Exception e)
                {
                    transaction.Rollback();
                }
            }
        }
}
}
