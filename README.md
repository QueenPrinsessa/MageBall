# MageBall

Unity version: 2020.3.0f1
https://unity3d.com/get-unity/download/archive

För att öppna med Unity Hub:
1. Ladda ner projektet
2. Packa upp projektet till en mapp
3. I Unity Hub under Projects klicka på knappen "Add" och välj mappen med projektet
4. Klicka på projektet för att öppna det
5. Öppna MainMenu scenen under _MageBall -> Scenes

Köra spelet själv:
Tryck på Create Game, Toggle Ready och tryck sedan Start Game för att komma in i spelet.

Testa multiplayerfunktionalitet:

Alla:
Skriv in ett namn för att få tillgång till host & join funktionalitet.

Host:
(Notera att port 7777 kan behöva port forwardas för att köra med någon annan online)
1. Hosta ett game genom att trycka create game.
2. Starta spelet när alla spelare har joinat och ready up.

Players:
1. Skriver in hostens ip address (sök: "What's my IP?" i google eller liknande) och joina.
2. Ready up.

För att testa lokalt (på en dator):
1. Skapa en build (ctrl+shift+b) med scenerna MainMenu som första, och Arena_01 som den andra
2. Starta projektet i playmode och builden samtidigt (självklart kan ni starta builden flera gånger och ansluta flera spelare)
3. Hosta på en av klienterna
4. Joina på den andra

I mappen /Assets/\_MageBall/Scripts/ ligger vår kod.
