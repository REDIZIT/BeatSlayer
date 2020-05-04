using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float m_JumpForce = 400f;
	public float m_CrouchSpeed = .36f;
	public float m_MovementSmoothing = .05f;
	
	public Rigidbody2D m_Rigidbody2D;
	private bool m_FacingRight = true;
	private Vector3 m_Velocity;

	private bool isOnGround;


	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space)) Jump();

		float moveInput = Input.GetAxis("Horizontal");
		Move(moveInput);

		isOnGround = false;
	}

	public void Move(float move)
	{
		Vector3 targetVelocity = new Vector2(move * 10f, m_Rigidbody2D.velocity.y);
		m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);
		
		if (move > 0 && !m_FacingRight)
		{
			Flip();
		}
		else if (move < 0 && m_FacingRight)
		{
			Flip();
		}
	}

	void Jump()
	{
		if (isOnGround)
		{
			m_Rigidbody2D.AddForce(Vector2.up * m_JumpForce);	
		}
	}


	private void Flip()
	{
		m_FacingRight = !m_FacingRight;
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
	}


	private void OnCollisionStay2D(Collision2D other)
	{
		isOnGround = true;
	}
}
