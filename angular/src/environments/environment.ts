export const environment = {
  production: false,
  application: {
    name: 'Core',
    logoUrl: '',
  },
  oAuthConfig: {
    issuer: 'https://localhost:44332',
    clientId: 'Core_App',
    dummyClientSecret: '1q2w3e*',
    scope: 'Core',
    showDebugInformation: true,
    oidc: false,
    requireHttps: true,
  },
  apis: {
    default: {
      url: 'https://localhost:44332',
    },
  },
  localization: {
    defaultResourceName: 'Core',
  },
};
