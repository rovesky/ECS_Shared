using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Entities;
using UnityEngine.Profiling;

[DisableAutoCreation]
public class HandleServerProjectileRequests :ComponentSystem
{
	private EntityQuery Group;

	public HandleServerProjectileRequests(BundledResourceManager resourceSystem) 
	{
		m_resourceSystem = resourceSystem;
    
		m_settings = Resources.Load<ProjectileModuleSettings>("ProjectileModuleSettings");
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		Group = GetEntityQuery(typeof(ProjectileRequest));
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Resources.UnloadAsset(m_settings);
	}

	protected override void OnUpdate()
	{
		var entityArray = Group.ToEntityArray(Allocator.Persistent);
		var requestArray = Group.ToComponentDataArray<ProjectileRequest>(Allocator.Persistent);
		
		// Copy requests as spawning will invalidate Group 
		var requests = new ProjectileRequest[requestArray.Length];
		for (var i = 0; i < requestArray.Length; i++)
		{
			requests[i] = requestArray[i];
			PostUpdateCommands.DestroyEntity(entityArray[i]);
		}

		// Handle requests
		var projectileRegistry = m_resourceSystem.GetResourceRegistry<ProjectileRegistry>();
		foreach (var request in requests)
		{
			var registryIndex = projectileRegistry.FindIndex(request.projectileAssetGuid);
			if (registryIndex == -1)
			{
				GameDebug.LogError("Cant find asset guid in registry");
				continue;
			}

			var projectileEntity = m_settings.projectileFactory.Create(EntityManager,m_resourceSystem, null);

			var projectileData = EntityManager.GetComponentData<ProjectileData>(projectileEntity);
			projectileData.SetupFromRequest(request, registryIndex);
			projectileData.Initialize(projectileRegistry);
			
			PostUpdateCommands.SetComponent(projectileEntity, projectileData);
			PostUpdateCommands.AddComponent(projectileEntity, new UpdateProjectileFlag());
		}
        entityArray.Dispose();
        requestArray.Dispose();

    }

	BundledResourceManager m_resourceSystem;
	ProjectileModuleSettings m_settings;
}
