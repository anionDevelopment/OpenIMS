// @ts-check
const eslint = require("@eslint/js");
const tseslint = require("typescript-eslint");
const angular = require("angular-eslint");

module.exports = tseslint.config(
  {
    /*
     * The api-client of the backend is generated (see the hints of this codeunit) and is overwritten on every
     * regeneration, so linting it would only report findings which can not be fixed here.
     */
    ignores: ["src/app/generated/**"],
  },
  {
    files: ["**/*.ts"],
    extends: [
      eslint.configs.recommended,
      ...tseslint.configs.recommended,
      ...tseslint.configs.stylistic,
      ...angular.configs.tsRecommended,
    ],
    processor: angular.processInlineTemplates,
    rules: {
      "@angular-eslint/directive-selector": [
        "error",
        {
          type: "attribute",
          prefix: "app",
          style: "camelCase",
        },
      ],
      "@angular-eslint/component-selector": [
        "error",
        {
          type: "element",
          prefix: "app",
          style: "kebab-case",
        },
      ],
      /*
       * This application is deliberately built with NgModules (see "home-page.module.ts",
       * "user-area.module.ts" and "admin-area.module.ts"), which group the components of the three areas of
       * the application and decide which of them are visible outside of their area. Every component is
       * therefore declared in a module and opts out of standalone on purpose, so this rule would report every
       * single component of the application.
       */
      "@angular-eslint/prefer-standalone": "off",
    },
  },
  {
    files: ["**/*.html"],
    extends: [
      ...angular.configs.templateRecommended,
      ...angular.configs.templateAccessibility,
    ],
    rules: {},
  }
);
