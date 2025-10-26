## Inspiration

We were inspired by the lore of **Knight Hacks 2025** and the **Google Gemini challenges**. We wanted to test ourselves with something novel, so we decided to create a virtual reality game which is hosted by an agent that **respond to player actions in real time**. 

## What it does

**T.K.’s Snackrifice** is a virtual reality game show where players face absurd challenges trying to defeat Lenny the dragon. The host, **T.K.**, delivers spontaneous commentary, roasts, and encouragement. Creating a fact paced, **reactive experience** that feels alive.

## How we built it

We used **Unity 6** and the **Meta Quest 3** for VR development. We integrated **Vertex AI's Gemini API** through a .NET backend to generate
dynamic real-time dialogue. We used **multiple AI agents** to make this possible, each with a distinct role: one defines the host's **personality and tone**, another generated **dynamic dialogue** through Google Gemini, and a third converts that **text to speech** using a TTS pipeline. Together, they allow our virtual host to think, speak, and perform like a real game show announcer. These outputs stream back into Unity in real time, **syncing voice, motion, and gameplay** for a seamless, reactive experience.

## Challenges we ran into

- Integrating Google Gemini and Eleven Labs directly within Unity for real-time interactions

- Implementing Eleven Labs' new v3 models using our custom-built client

## Accomplishments that we're proud of

- Tackling a project of this magnitude with no Gemini experience

- Brought **Google Gemini** into a **Unity VR experience**, a first for us and something we haven’t seen elsewhere, transforming it into a fully interactive in-engine AI host that reacts live to players.

- How we seamlessly merged our individually developed systems into one working product. Each of us tackled different parts of the project and filled in gaps for one another, and integration was smooth with almost no conflict.

## What we learned

We learned how to connect a **real-time AI agent** with an interactive 3D environment, work with Gemini's API, and create dynamic interactions by **sending flags to dictate AI prompting/behavior**. We've also improved our understanding of Unity and scene creation. 

## What's next for T.K.’s Snackrifice

We're very happy with the final version of our project, but we also see room for growth. In the future, we'd like to add **more mini-games** to make the experience more varied and replayable. Since a core part of T.K.’s Snackrifice is the AI host's ability to respond to player actions, expanding the number of in-game events and interaction flags would make those responses feel even more natural and engaging.
