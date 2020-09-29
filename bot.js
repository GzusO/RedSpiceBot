const tmi = require('tmi.js');

config_data = {}//LOAD JSON

config_data = require('./config/config.json')

var spice = 0
// Create a client with our options
const client = new tmi.client(config_data);

// Register our event handlers (defined below)
client.on('message', onMessageHandler);
client.on('connected', onConnectedHandler);

// Connect to Twitch:
client.connect();

// Called every time a message comes in
function onMessageHandler (target, context, msg, self) {
  if (self) { return; } // Ignore messages from the bot

  // Remove whitespace from chat message
  const commandName = msg.trim();

  // If the command is known, let's execute it
  if (commandName.ToLowerCase() === '!spice') {
    spice += 1;
    client.say(target, `This rare spice is used as a flavoring for certain foods and often produces an eye-watering effect on the consumer. (Quantity: ${spice})`);
    console.log(`* Executed ${commandName} command`);
  } else {
    console.log(`* Unknown command ${commandName}`);
  }
}


// Called every time the bot connects to Twitch chat
function onConnectedHandler (addr, port) {
  console.log(`* Connected to ${addr}:${port}`);
}
