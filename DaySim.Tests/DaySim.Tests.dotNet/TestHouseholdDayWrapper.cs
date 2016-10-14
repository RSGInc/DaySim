using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Daysim.DomainModels;
using Daysim.DomainModels.Default.Models;
using Daysim.DomainModels.Default.Wrappers;
using Daysim.DomainModels.Factories;
using Daysim.Framework.Core;
using Daysim.Framework.DomainModels.Creators;
using Daysim.Framework.DomainModels.Models;
using Daysim.Framework.DomainModels.Persisters;
using Daysim.Framework.DomainModels.Wrappers;
using Daysim.Framework.Persistence;
using SimpleInjector;

namespace Daysim.Tests {
	public class TestHouseholdDayWrapper : HouseholdDayWrapper
	{
		public TestHouseholdDayWrapper(IHouseholdDay householdDay, IHouseholdWrapper householdWrapper) : base(householdDay, householdWrapper, new PersonDayWrapperFactory())
		{
		}

		protected override IPersonDayWrapper CreatePersonDay(IPersonWrapper person)
		{
			return new PersonDayWrapper(new PersonDay
				                            {
					                            Id = person.Id, // ++_nextPersonDayId,
					                            PersonId = person.Id,
					                            HouseholdDayId = _householdDay.Id,
					                            HouseholdId = _householdDay.HouseholdId,
					                            PersonSequence = person.Sequence,
					                            Day = _householdDay.Day
				                            }, person, this);
		}
	}

		public class TestHouseholdPersister : IHouseholdPersister
	{

		#region IHouseholdPersister Members

		public System.Collections.Generic.IEnumerable<IHousehold> Seek(int id, string householdFk) {
			throw new NotImplementedException();
		}

		public void Export(IHousehold household) {
			throw new NotImplementedException();
		}

		public void Dispose() {
			throw new NotImplementedException();
		}

		public void BeginImport(IImporterFactory importerFactory, string path, string message) {
			throw new NotImplementedException();
		}

		public int Count {
			get { throw new NotImplementedException(); }
		}

		#endregion

		#region IEnumerable<IHousehold> Members

		public System.Collections.Generic.IEnumerator<IHousehold> GetEnumerator() {
			throw new NotImplementedException();
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
			throw new NotImplementedException();
		}

		#endregion
	}

	public class TestPersonPersister : IPersonPersister
	{
		private List<IPerson> _persons; 
		public TestPersonPersister(List<IPerson> persons)
		{
			_persons = persons;
		}

		#region IPersonPersister Members

		public System.Collections.Generic.IEnumerable<IPerson> Seek(int id, string householdFk) {
			
			return _persons;
		}

		public void Export(IPerson person) {
			
		}

		public void Dispose() {
			
		}

		public void BeginImport(IImporterFactory importerFactory, string path, string message) {
			
		}

		public void BuildIndex(string indexName, string idName, string parentIdName) {
			
		}

		#endregion
	}

	public class TestHouseholdDayPersister : IHouseholdDayPersister
	{

		#region IHouseholdDayPersister Members

		public System.Collections.Generic.IEnumerable<IHouseholdDay> Seek(int id, string householdFk) {
			throw new NotImplementedException();
		}

		public void Export(IHouseholdDay householdDay) {
			throw new NotImplementedException();
		}

		public void Dispose() {
			throw new NotImplementedException();
		}

		public void BeginImport(IImporterFactory importerFactory, string path, string message) {
			throw new NotImplementedException();
		}

		public void BuildIndex(string indexName, string idName, string parentIdName) {
			throw new NotImplementedException();
		}

		#endregion
	}

	public class TestHouseholdDayWrapperCreator : IHouseholdDayWrapperCreator
	{

		#region IHouseholdDayWrapperCreator Members

		public IHouseholdDayWrapper CreateWrapper(IHouseholdDay householdDay, IHouseholdWrapper householdWrapper) {
			return new TestHouseholdDayWrapper(householdDay, householdWrapper);
		}

		#endregion
	}

	public class TestPersonWrapperCreator : IPersonWrapperCreator
	{

		#region IPersonWrapperCreator Members

		public IPersonWrapper CreateWrapper(IPerson person, IHouseholdWrapper household) {
			return new PersonWrapper(person, household);
		}

		#endregion
	}

	public class TestPersonDayWrapperCreator : IPersonDayWrapperCreator
	{

		#region IPersonDayWrapperCreator Members

		public IPersonDayWrapper CreateWrapper(IPersonDay personDay, IPersonWrapper personWrapper, IHouseholdDayWrapper householdDayWrapper) {
			throw new NotImplementedException();
		}

		#endregion
	}
}
