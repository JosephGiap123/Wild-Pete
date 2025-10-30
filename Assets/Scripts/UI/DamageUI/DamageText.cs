using UnityEngine;
using TMPro;
using System.Collections;
using System.Threading.Tasks;
using Unity.VisualScripting;
public class DamageText : MonoBehaviour
{
    private int damage = 0;
    private Vector2 curVelocity = new(0f, 0f);
    Animator anim;
    Rigidbody2D rb;

    public void Initialize(Vector2 initVel, int dmg, Color fill, Color outline)
    {
        GetComponent<TMP_Text>().faceColor = fill;
        GetComponent<TMP_Text>().outlineColor = outline;
        damage = dmg;
        curVelocity = initVel;
        rb = transform.parent.gameObject.GetComponent<Rigidbody2D>();
        GetComponent<TMP_Text>().text = '-' + dmg.ToString();
        anim = GetComponent<Animator>();
        rb.linearVelocity = new(Mathf.Clamp(curVelocity.x, -4f, 4f), curVelocity.y);
        rb.linearVelocity = new(rb.linearVelocity.x * Random.Range(0.8f, 1.2f), curVelocity.y * Random.Range(0.8f, 1.2f));
        StartCoroutine(StartFading());
    }

    public IEnumerator StartFading()
    {
        yield return new WaitForSeconds(0.5f);
        anim.Play("DisappearText");
    }

    public void DeleteDamageText()
    {
        Destroy(transform.parent.gameObject);
    }
}
