using UnityEngine;

public class PlayerControl : MonoBehaviour
{
	public float speed = 3.0f;

	private bool canControl = false;
	private string networkID;
	private Vector3 inputVector;
	private float h;
	private Animator anim;
	private bool flipX = false;
	private SpriteRenderer sprite;
	private Rigidbody2D rigid;

	private void Start()
	{
		if (canControl)
			InvokeRepeating("UpdateInput", 1, 1.0f / 60.0f);
		anim = GetComponent<Animator>();
		sprite = GetComponent<SpriteRenderer>();
		rigid = GetComponent<Rigidbody2D>();
	}

	private void Update()
	{
		if (canControl)
		{
			Jump();
			h = Input.GetAxisRaw("Horizontal");
			transform.Translate(h * Time.deltaTime * speed, 0, 0);

			inputVector = transform.position;

			float absH = Mathf.Abs(h);
			anim.SetFloat("speed", absH);

			FlipImage(h);
		}
	}
	public void SetNetID(string id)
	{
		networkID = id;
	}
	public void SetControl(bool control)
	{
		canControl = control;
	}
	public void UpdateInput()
	{
		NetworkClient.Instance.SendInput(inputVector, flipX);
	}
	void Jump()
    {
		if(Input.GetKeyDown(KeyCode.Space))
        {
			rigid.AddForce(Vector3.up * 10, ForceMode2D.Impulse);
            FindObjectOfType<SoundManager>().Play("jump");
        }
    }

	private void FlipImage(float h)
	{
		if (h < 0)
		{
			flipX = true;
		}
		else if (h > 0)
		{
			flipX = false;
		}

		if (flipX)
		{
			sprite.flipX = true;
		}
		else
		{
			sprite.flipX = false;
		}
	}
}
