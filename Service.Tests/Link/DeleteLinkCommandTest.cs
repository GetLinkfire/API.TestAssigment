using FizzWare.NBuilder;
using NUnit.Framework;
using Repository;
using Repository.Entities;
using Repository.Interfaces;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Service.Tests.Link
{
	[TestFixture]
	[ExcludeFromCodeCoverage]
	public class DeleteLinkCommandTest
	{
        private Repository.Entities.Context _context;
        private ILinkRepository _repository;

        [SetUp]
        public void Init()
        {
            _context = new Context();
            _repository = new LinkRepository(_context);
        }

        [TearDown]
        public void Cleanup()
        {
            _context.Dispose();
        }

        // TODO: implement delete link command + unit tests
    }
}
