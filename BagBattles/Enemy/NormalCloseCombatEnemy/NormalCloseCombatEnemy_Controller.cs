public class NormalCloseCombatEnemy_Controller : EnemyController
{
    protected override void Start()
    {
        base.Start();
        // Initialize enemy-specific properties or behaviors here
        enemy_type = Enemy.EnemyType.NormalCloseCombatEnemy;
    }
}
