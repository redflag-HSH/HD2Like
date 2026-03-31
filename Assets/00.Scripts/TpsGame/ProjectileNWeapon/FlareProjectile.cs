using UnityEngine;

public class FlareProjectile : Projectile
{
    float _lifetime = 20.0f;
    float _groundDistance = .9f;
    [Space(5)]
    [SerializeField] GameObject _smoke;
    [SerializeField] GameObject _snowRemover, _light;
    [SerializeField] float SkyLevel;
    bool _onGround;
    protected override void Update()
    {
        if (!_onGround)
        {
            base.Update();
            CheckGround();
            SkyLevelCheck();
        }
    }
    private void CheckGround()
    {
        Ray ray = new Ray(transform.position, -transform.up);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, _groundDistance))
            GotOnGround();
    }
    public void GotOnGround()
    {
        transform.rotation = Quaternion.Euler(0, 0, 0);
        _onGround = true;
        _smoke.SetActive(true);
        _snowRemover.SetActive(true);
        Invoke("Disable", _lifetime);
    }
    private void Disable()
    {
        Debug.Log("detach");
        _light.transform.parent = null;
        _light.GetComponent<FlareLightFlicker>().enabled = false;
        _light.GetComponent<LightFade>().FadeStart();
        Debug.Log("flare disable");
        DestroyProjectile();
    }
    private void SkyLevelCheck()
    {
        if (transform.position.y >= SkyLevel)
        {

        }
    }
}
