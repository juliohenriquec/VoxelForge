using Godot;
using System;
using Godot.Collections;

public partial class Player : CharacterBody3D
{
	[Export] public float Speed = 20.0f; // velocidade do player
	[Export] public float MouseSensitivity = 0.002f; //sensibilidade do mouse
	
	Camera3D camera;
	float pitch = 0.0f;
	
	public override void _Ready()
	{
		camera = GetNode<Camera3D>("Camera3D");
		Input.MouseMode = Input.MouseModeEnum.Captured;
		SetProcessInput(true);
	}

	public override void _Input(InputEvent e)
	{
		if (e is InputEventMouseMotion motion)
		{
			RotateY(-motion.Relative.X * MouseSensitivity); // rotação horizontal
			
			pitch -= motion.Relative.Y * MouseSensitivity; //Rotação vertical
			pitch = Mathf.Clamp(pitch, -1.5f, 1.5f); //limite da rotação
			
			camera.Rotation = new Vector3(pitch, 0.0f, 0.0f); //define a rotação da camera

		}
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector3 direction = Vector3.Zero;
		
		if (Input.IsActionPressed("move_forward"))
			direction -= Transform.Basis.Z;
		if (Input.IsActionPressed("move_back"))
			direction += Transform.Basis.Z;
		if (Input.IsActionPressed("move_left"))
			direction -= Transform.Basis.X;
		if (Input.IsActionPressed("move_right"))
			direction += Transform.Basis.X;
		if (Input.IsActionPressed("move_up"))
			direction += Transform.Basis.Y;
		if (Input.IsActionPressed("move_down"))
			direction -= Transform.Basis.Y;
		
		Position += direction.Normalized() * Speed * (float)delta;
		MoveAndSlide(); 
		
		if (Input.IsActionPressed("liberar_mouse"))
			Input.MouseMode = Input.MouseModeEnum.Visible;
			
	}
}
