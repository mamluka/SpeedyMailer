using System.Collections.Generic;

namespace SpeedyMailer.Drone.Tasks
{
	public class ScheduledTaskDataProvider
	{
		private readonly IDictionary<string, object> _dataDictionay = new Dictionary<string, object>();

		public void SetData(ScheduledTaskData data)
		{
			if (data == null) return;
			_dataDictionay.Add(data.GetType().FullName, data);
		}

		public T GetData<T>(string key) where T : class
		{
			return _dataDictionay[key] as T;
		}
	}
}