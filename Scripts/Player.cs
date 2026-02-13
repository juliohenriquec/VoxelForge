using Godot;
using System;

public partial class Player : Node3D
{
	[Export] public float Speed = 10.0f;
	[Export] public float MouseSensitivity = 0.002f;
	
	Camera3D camera;
	float pitch = 0.0f;
	
	
	// Called when the node enters the scene tree for the first time.
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
			RotateY(-motion.Relative.X * MouseSensitivity);
			
			pitch -= motion.Relative.Y * MouseSensitivity;
			pitch = Mathf.Clamp(pitch, -1.5f, 1.5f);
			
			camera.Rotation = new Vector3(pitch, 0.0f, 0.0f);
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
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
	}
}
