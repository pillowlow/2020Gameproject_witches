
public class DuskKnightShield : Enemy
{
    protected override void Die()
    {
        state = StateCode.Die;
        Destroy(gameObject);
    }
}