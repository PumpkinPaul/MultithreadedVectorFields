using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MoonTools.ECS;
using MultithreadedVectorFields.Engine;
using MultithreadedVectorFields.Engine.Extensions;
using MultithreadedVectorFields.Engine.Threading;
using MultithreadedVectorFields.Gameplay;
using MultithreadedVectorFields.Gameplay.GameMaps;
using MultithreadedVectorFields.Gameplay.Renderers;
using MultithreadedVectorFields.Gameplay.Systems;
using System;
using System.Collections.Generic;

namespace MultithreadedVectorFields;

/// <summary>
/// Experimental FNA project for generating agent vector fields in a multithreaded manner - with no runtime heap allocations
/// </summary>
public class MultithreadedVectorFieldsGame : BaseGame
{
    readonly Dungeon _dungeon = new();

    //------------------------------------------------------------------------------------------------------------------------------------------------------ 
    //ECS
    World _world;

    //Systems
    MoonTools.ECS.System[] _systems;

    //Renderers
    SpriteRenderer _spriteRenderer;
    VectorFieldRenderer _vectorFieldRenderer;

    CalculateVectorFieldsJob _calculateVectorFieldsJob;
    readonly Dictionary<Entity, VectorField> _vectorFields = new();
    
    int _visibleVectorFieldIdx = 0;

    public CustomThreadPoolComponent ThreadPool { get; }

    public MultithreadedVectorFieldsGame()
    {
        Window.Title = "Multi-threaded Vector Fields";

        ThreadPool = new CustomThreadPoolComponent(this);
        Components.Add(ThreadPool);
    }

    protected override void OnLoadContent()
    {
        base.OnLoadContent();

        _dungeon.LoadContent();
    }

    protected override void BeginRun()
    {
        base.BeginRun();

        _world = new World();
        _calculateVectorFieldsJob = new(ThreadPool, _vectorFields, _dungeon, 32, 32);

        _systems = new MoonTools.ECS.System[]
        {
            //Assign calculated vector fields to agents
            new ConsumeVectorFieldSystem(_world, _vectorFields, _calculateVectorFieldsJob),

            //Spawn the entities into the game world
            new AgentSpawnSystem(_world),

            //Move the entities in the world
            //Note - no movement implemented - might add some moving entities later
            new MovementSystem(_world),

            //Calculate the vector field for agents
            new CreateVectorFieldSystem(_world, _calculateVectorFieldsJob),

            //Remove the dead entities
            //Note - no deaths implemented
            //- using Entity as a key into the vector fields dictionary so work would be required here if agents die
            //- could also implement cancelling jobs for dead entities
            new DestroyEntitySystem(_world)
        };

        _spriteRenderer = new SpriteRenderer(_world, SpriteBatch);
        _vectorFieldRenderer = new VectorFieldRenderer(_world, SpriteBatch, _vectorFields);
        
        SpawnAgents();
    }

    private void SpawnAgents()
    {
        var random = new FastRandom();
        const int AGENT_COUNT = 128;

        for (var i = 0; i < AGENT_COUNT; i++)
        {
            var position = Vector2.Zero;

            while (_dungeon.IsTileSolid(position))
            {
                position = new Vector2(
                    random.Next(0, (int)TileMap.MAX_COLUMNS),
                    random.Next(0, (int)TileMap.MAX_ROWS));

                position *= TileMap.TILE_SIZE;
            };

            _world.Send(new AgentSpawnMessage(
                Position: position + new Vector2(TileMap.TILE_SIZE) / 2,
                Color.Green
            ));
        }
    }

    protected override void OnUpdate(GameTime gameTime)
    {
        if (KeyboardState.IsKeyDown(Keys.Escape) && PreviousKeyboardState.IsKeyUp(Keys.Escape))
            Exit();

        //----------------------------------------------------------------------------------------------------
        //Toggle through the different vector fields - the active one will be visualized
        if (_visibleVectorFieldIdx < _vectorFields.Count -1 && KeyboardState.IsKeyDown(Keys.OemPlus) && PreviousKeyboardState.IsKeyUp(Keys.OemPlus))
            _visibleVectorFieldIdx++;

        if (_visibleVectorFieldIdx > 0 && KeyboardState.IsKeyDown(Keys.OemMinus) && PreviousKeyboardState.IsKeyUp(Keys.OemMinus))
            _visibleVectorFieldIdx--;

        //----------------------------------------------------------------------------------------------------
        //ECS world update
        var delta = TimeSpan.FromSeconds(1000 / 60.0f);
        foreach (var system in _systems)
            system.Update(delta);

        _world.FinishUpdate();
    }

    protected override void OnDraw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        //Draw the world - Ortho off centre

        //                         width, height
        //     -------------------------
        //     |                       |
        //     |                       |
        //     |                       |
        //     |                       |
        //     |                       |
        //     |                       |
        //     -------------------------
        // 0, 0

        SpriteBatch.Begin(0, BlendState.AlphaBlend, null, null, RasterizerState.CullClockwise, BasicEffect);

        //...gamemap
        _dungeon.Draw(SpriteBatch);

        //...vector field visualisations
        _vectorFieldRenderer.Draw(_visibleVectorFieldIdx);

        //...all the entities
        _spriteRenderer.Draw();

        SpriteBatch.End();

        //Draw the 'UI'
        var text = "MULTITHREADED VECTOR FIELDS";
        var position = new Vector2(SCREEN_WIDTH * 0.5f, 430);
        SpriteBatch.BeginTextRendering();
        SpriteBatch.DrawText(Resources.SmallFont, text, position, Color.White, Alignment.Centre);
        SpriteBatch.DrawText(Resources.SmallFont, text, position + new Vector2(1, -1), Color.Black, Alignment.Centre);
        SpriteBatch.End();
    }
}
