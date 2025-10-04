# Simulation Engine

A modular simulation engine implemented in C#. This project provides core infrastructure and modules for world simulation, economy, and sandbox/testing support.

---

## Table of Contents

- [About](#about)  
- [Features](#features)  
- [Architecture](#architecture)

---

## About

The simulation-engine repository (aka *medieval-sim*) is organized around a core engine, world simulation modules, economy modules, and a sandbox/test suite. It aims to provide a flexible framework through which simulation scenarios like resource management, trade, and world dynamics can be built and experimented with.

---

## Features

- Core ECS (Entity-Component-System) framework via `medieval-sim.core`  
- World & economy simulation modules: population, markets, trade, etc.  
- Sandbox / test project for experimentation / validation  
- Well-structured architecture separating core, modules, and testing  

---

## Architecture

The code is split into several main projects/modules:

| Project | Purpose |
|---|---|
| `medieval-sim.core` | Core engine functionality: ECS, scheduling, basic simulation primitives |
| `medieval-sim.modules.world` | World simulation: geography or social dynamics, settlements, world state |
| `medieval-sim.modules.economy` | Economic logic: markets, resources, trade, supply & demand |
| `medieval-sim.sandbox` | For trying out and prototyping simulations; example scenarios |
| `medieval-sim.tests` | Unit tests and validation of simulation logic |

---
