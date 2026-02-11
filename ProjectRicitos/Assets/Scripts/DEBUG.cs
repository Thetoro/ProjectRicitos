using TMPro;
using UnityEngine;

public class DEBUG : MonoBehaviour
{
    [SerializeField]
    private TMP_Text velocidadX;
    [SerializeField]    
    private TMP_Text velocidadY;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        velocidadX.text = "X: " + rb.linearVelocityX.ToString();
        velocidadY.text = "Y: " + rb.linearVelocityY.ToString();
    }
}
