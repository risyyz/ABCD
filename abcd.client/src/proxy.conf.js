const PROXY_CONFIG = [
  {
    context: [
      "/api"
    ],
    target: "https://localhost:7021",
    secure: false
  }
]

module.exports = PROXY_CONFIG;
