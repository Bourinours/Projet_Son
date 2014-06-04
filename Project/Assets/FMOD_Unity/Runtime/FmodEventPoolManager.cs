using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FmodEventPoolManager
{
	private Dictionary<FmodEvent, FmodEventPool> m_pools = new Dictionary<FmodEvent, FmodEventPool>();
	
	public FmodEventPool getEventPool(FmodEvent evt) {
		if (m_pools.ContainsKey(evt)) {
			return (m_pools[evt]);
		}
		FmodEventPool newPool = new FmodEventPool(evt);
		m_pools[evt] = newPool;
		return (newPool);
	}
	
	public bool eventPoolExists(FmodEvent evt) {
		return (m_pools.ContainsKey(evt));
	}
	
	public FmodEventPool[] getAllPools() {
		FmodEventPool[] ret = new FmodEventPool[m_pools.Count];
		m_pools.Values.CopyTo(ret, 0);
		return (ret);
	}
	
	public List<FmodEventAudioSource> getAllActiveSources() {
		List<FmodEventAudioSource> list = new List<FmodEventAudioSource>();
		
		foreach (KeyValuePair<FmodEvent, FmodEventPool> pair in m_pools) {
			list.AddRange(pair.Value.getActiveSources());
		}
		return (list);
	}
}

