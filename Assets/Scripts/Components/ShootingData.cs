using Unity.Entities;

[GenerateAuthoringComponent]
public struct ShootingData : IComponentData
{
    public Entity bulletPrefab;
    public bool canShoot;
    public bool manualShooting;
}
